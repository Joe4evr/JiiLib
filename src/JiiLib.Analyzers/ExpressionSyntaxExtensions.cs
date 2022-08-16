using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JiiLib.Analyzers;

// relevant extension methods copied from
// https://sourceroslyn.io/#Microsoft.CodeAnalysis.CSharp.Workspaces/J/s/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs/ExpressionSyntaxExtensions.cs
// because these aren't public for some reason....
internal static class ExpressionSyntaxExtensions
{
    public static bool IsWrittenTo(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (expression == null)
            return false;

        expression = GetExpressionToAnalyzeForWrites(expression);

        if (expression.IsOnlyWrittenTo())
            return true;

        if (expression.IsInRefContext(out var refParent))
        {
            // most cases of `ref x` will count as a potential write of `x`.  An important exception is:
            // `ref readonly y = ref x`.  In that case, because 'y' can't be written to, this would not 
            // be a write of 'x'.
            if (refParent.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Type: RefTypeSyntax refType } } }
                && refType.ReadOnlyKeyword != default)
            {
                return false;
            }

            return true;
        }

        // Similar to `ref x`, `&x` allows reads and write of the value, meaning `x` may be (but is not definitely)
        // written to.
        if (expression.Parent.IsKind(SyntaxKind.AddressOfExpression))
            return true;

        // We're written if we're used in a ++, or -- expression.
        if (expression.IsOperandOfIncrementOrDecrementExpression())
            return true;

        if (expression.IsLeftSideOfAnyAssignExpression())
            return true;

        // An extension method invocation with a ref-this parameter can write to an expression.
        if (expression.Parent is MemberAccessExpressionSyntax memberAccess &&
            expression == memberAccess.Expression)
        {
            var symbol = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol;
            if (symbol is IMethodSymbol { MethodKind: MethodKind.ReducedExtension, ReducedFrom: IMethodSymbol reducedFrom } &&
                reducedFrom.Parameters.Length > 0 &&
                reducedFrom.Parameters.First().RefKind == RefKind.Ref)
            {
                return true;
            }
        }

        return false;
    }

    private static ExpressionSyntax GetExpressionToAnalyzeForWrites(ExpressionSyntax expression)
    {
        if (expression.IsRightSideOfDotOrArrow())
        {
            expression = (ExpressionSyntax)expression.GetRequiredParent();
        }

        expression = expression.WalkUpParentheses();

        return expression;
    }
    private static bool IsOnlyWrittenTo(this ExpressionSyntax expression)
    {
        expression = GetExpressionToAnalyzeForWrites(expression);

        if (expression != null)
        {
            if (expression.IsInOutContext())
            {
                return true;
            }

            if (expression.Parent != null)
            {
                if (expression.IsLeftSideOfAssignExpression())
                {
                    return true;
                }

                if (expression.IsAttributeNamedArgumentIdentifier())
                {
                    return true;
                }
            }

            if (IsExpressionOfArgumentInDeconstruction(expression))
            {
                return true;
            }
        }

        return false;
    }
    private static bool IsExpressionOfArgumentInDeconstruction(ExpressionSyntax expr)
    {
        if (!expr.IsParentKind(SyntaxKind.Argument))
        {
            return false;
        }

        while (true)
        {
            var parent = expr.Parent;
            if (parent == null)
            {
                return false;
            }

            switch (parent.Kind())
            {
                case SyntaxKind.Argument:
                    if (parent.Parent?.IsKind(SyntaxKind.TupleExpression) == true)
                    {
                        expr = (TupleExpressionSyntax)parent.Parent;
                        continue;
                    }

                    return false;
                case SyntaxKind.SimpleAssignmentExpression:
                    if (((AssignmentExpressionSyntax)parent).Left == expr)
                    {
                        return true;
                    }

                    return false;
                case SyntaxKind.ForEachVariableStatement:
                    if (((ForEachVariableStatementSyntax)parent).Variable == expr)
                    {
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }
    }
    private static bool IsParentKind([NotNullWhen(returnValue: true)] this SyntaxNode? node, SyntaxKind kind)
            => Microsoft.CodeAnalysis.CSharpExtensions.IsKind(node?.Parent, kind);
    private static bool IsInRefContext([NotNullWhen(true)] this ExpressionSyntax? expression, [NotNullWhen(true)] out SyntaxNode? refParent)
    {
        while (expression?.Parent is ParenthesizedExpressionSyntax or PostfixUnaryExpressionSyntax(SyntaxKind.SuppressNullableWarningExpression))
            expression = (ExpressionSyntax)expression.Parent!;

        if (expression?.Parent is RefExpressionSyntax or
                                  ArgumentSyntax { RefOrOutKeyword.RawKind: (int)SyntaxKind.RefKeyword })
        {
            refParent = expression.Parent;
            return true;
        }

        refParent = null;
        return false;
    }
    private static bool IsOperandOfIncrementOrDecrementExpression(this ExpressionSyntax expression)
    {
        if (expression?.Parent is SyntaxNode parent)
        {
            switch (parent.Kind())
            {
                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                case SyntaxKind.PreDecrementExpression:
                    return true;
            }
        }

        return false;
    }
    private static bool IsLeftSideOfAnyAssignExpression([NotNullWhen(true)] this SyntaxNode? node)
    {
        return node?.Parent != null &&
            node.Parent.IsAnyAssignExpression() &&
            ((AssignmentExpressionSyntax)node.Parent).Left == node;
    }
    private static bool IsAnyAssignExpression(this SyntaxNode node)
            => SyntaxFacts.IsAssignmentExpression(node.Kind());
    private static bool IsRightSideOfDotOrArrow(this ExpressionSyntax name)
            => IsAnyMemberAccessExpressionName(name) || IsRightSideOfQualifiedName(name);
    private static bool IsAnyMemberAccessExpressionName(this ExpressionSyntax expression)
    {
        if (expression == null)
        {
            return false;
        }

        return expression == (expression.Parent as MemberAccessExpressionSyntax)?.Name ||
            expression.IsMemberBindingExpressionName();
    }
    private static bool IsMemberBindingExpressionName([NotNullWhen(true)] this ExpressionSyntax? expression)
            => expression.IsParentKind(SyntaxKind.MemberBindingExpression, out MemberBindingExpressionSyntax? memberBinding) &&
               memberBinding.Name == expression;
    private static bool IsRightSideOfQualifiedName([NotNullWhen(true)] this ExpressionSyntax? expression)
            => expression.IsParentKind(SyntaxKind.QualifiedName, out QualifiedNameSyntax? qualifiedName) && qualifiedName.Right == expression;
    private static bool IsParentKind<TNode>([NotNullWhen(returnValue: true)] this SyntaxNode? node, SyntaxKind kind, [NotNullWhen(returnValue: true)] out TNode? result)
            where TNode : SyntaxNode
    {
        if (node.IsParentKind(kind))
        {
            result = (TNode)node.Parent!;
            return true;
        }

        result = null;
        return false;
    }
    private static SyntaxNode GetRequiredParent(this SyntaxNode node)
            => node.Parent ?? throw new InvalidOperationException("Node's parent was null");
    private static ExpressionSyntax WalkUpParentheses(this ExpressionSyntax expression)
    {
        while (expression.IsParentKind(SyntaxKind.ParenthesizedExpression, out ExpressionSyntax? parentExpr))
            expression = parentExpr;

        return expression;
    }
    private static bool IsInOutContext(this ExpressionSyntax expression)
    {
        return
            expression?.Parent is ArgumentSyntax argument &&
            argument.Expression == expression &&
            argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);
    }
    private static bool IsLeftSideOfAssignExpression([NotNullWhen(returnValue: true)] this SyntaxNode? node)
            => node.IsParentKind(SyntaxKind.SimpleAssignmentExpression, out AssignmentExpressionSyntax? assignment) &&
               assignment.Left == node;
    private static bool IsAttributeNamedArgumentIdentifier([NotNullWhen(true)] this ExpressionSyntax? expression)
    {
        var nameEquals = expression?.Parent as NameEqualsSyntax;
        return nameEquals.IsParentKind(SyntaxKind.AttributeArgument);
    }

    private static void Deconstruct(this PostfixUnaryExpressionSyntax syntax, out SyntaxKind syntaxKind) => syntaxKind = syntax.Kind();
}
