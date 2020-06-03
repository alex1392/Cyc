/*
 * MathConverter and accompanying samples are copyright (c) 2011 by Ivan Krivyakov
 * ivan [at] ikriv.com
 * They are distributed under the Apache License http://www.apache.org/licenses/LICENSE-2.0.html
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Cyc.FluentDesign.Converters {

	/// <summary>
	/// Value converter that performs arithmetic calculations over its argument(s)
	/// </summary>
	/// <remarks>
	/// MathConverter can act as a value converter, or as a multivalue converter (WPF only).
	/// It is also a markup extension (WPF only) which allows to avoid declaring resources,
	/// ConverterParameter must contain an arithmetic expression over converter arguments. Operations supported are +, -, * and /
	/// Single argument of a value converter may referred as x, a, or [0]
	/// Arguments of multi value converter may be referred as x,y,z,t (first-fourth argument), or a,b,c,d, or [0], [1], [2], [3], [4], ...
	/// The converter supports arithmetic expressions of arbitrary complexity, including nested subexpressions
	/// </remarks>
	public class MathConverter : MarkupExtension, IMultiValueConverter, IValueConverter {

		#region Private Interfaces

		private interface IExpression {

			#region Public Methods

			decimal Eval(object[] args);

			#endregion Public Methods
		}

		#endregion Private Interfaces

		#region Private Classes

		private class BinaryOperation : IExpression {

			#region Private Fields

			private readonly IExpression _left;
			private readonly Func<decimal, decimal, decimal> _operation;
			private readonly IExpression _right;

			#endregion Private Fields

			#region Public Constructors

			public BinaryOperation(char operation, IExpression left, IExpression right)
			{
				_left = left;
				_right = right;
				switch (operation) {
					case '+':
						_operation = (a, b) => (a + b);
						break;

					case '-':
						_operation = (a, b) => (a - b);
						break;

					case '*':
						_operation = (a, b) => (a * b);
						break;

					case '/':
						_operation = (a, b) => (a / b);
						break;

					default:
						throw new ArgumentException("Invalid operation " + operation);
				}
			}

			#endregion Public Constructors

			#region Public Methods

			public decimal Eval(object[] args)
			{
				return _operation(_left.Eval(args), _right.Eval(args));
			}

			#endregion Public Methods
		}

		private class Constant : IExpression {

			#region Private Fields

			private readonly decimal _value;

			#endregion Private Fields

			#region Public Constructors

			public Constant(string text)
			{
				if (!decimal.TryParse(text, out _value)) {
					throw new ArgumentException(string.Format("'{0}' is not a valid number", text));
				}
			}

			#endregion Public Constructors

			#region Public Methods

			public decimal Eval(object[] args)
			{
				return _value;
			}

			#endregion Public Methods
		}

		private class Negate : IExpression {

			#region Private Fields

			private readonly IExpression _param;

			#endregion Private Fields

			#region Public Constructors

			public Negate(IExpression param)
			{
				_param = param;
			}

			#endregion Public Constructors

			#region Public Methods

			public decimal Eval(object[] args)
			{
				return -_param.Eval(args);
			}

			#endregion Public Methods
		}

		private class Parser {

			#region Private Fields

			private int pos;
			private string text;

			#endregion Private Fields

			#region Public Methods

			public IExpression Parse(string text)
			{
				try {
					pos = 0;
					this.text = text;
					var result = ParseExpression();
					RequireEndOfText();
					return result;
				} catch (Exception ex) {
					var msg =
						string.Format("MathConverter: error parsing expression '{0}'. {1} at position {2}", text, ex.Message, pos);

					throw new ArgumentException(msg, ex);
				}
			}

			#endregion Public Methods

			#region Private Methods

			private IExpression CreateVariable(int n)
			{
				++pos;
				SkipWhiteSpace();
				return new Variable(n);
			}

			private IExpression ParseExpression()
			{
				var left = ParseTerm();

				while (true) {
					if (pos >= text.Length) {
						return left;
					}

					var c = text[pos];

					if (c == '+' || c == '-') {
						++pos;
						var right = ParseTerm();
						left = new BinaryOperation(c, left, right);
					} else {
						return left;
					}
				}
			}

			private IExpression ParseFactor()
			{
				SkipWhiteSpace();
				if (pos >= text.Length) {
					throw new ArgumentException("Unexpected end of text");
				}

				var c = text[pos];

				if (c == '+') {
					++pos;
					return ParseFactor();
				}

				if (c == '-') {
					++pos;
					return new Negate(ParseFactor());
				}

				if (c == 'x' || c == 'a') {
					return CreateVariable(0);
				}

				if (c == 'y' || c == 'b') {
					return CreateVariable(1);
				}

				if (c == 'z' || c == 'c') {
					return CreateVariable(2);
				}

				if (c == 't' || c == 'd') {
					return CreateVariable(3);
				}

				if (c == '(') {
					++pos;
					var expression = ParseExpression();
					SkipWhiteSpace();
					Require(')');
					SkipWhiteSpace();
					return expression;
				}

				if (c == '[') {
					++pos;
					var end = text.IndexOf(']', pos);
					if (end < 0) { --pos; throw new ArgumentException("Unmatched '['"); }
					if (end == pos) { throw new ArgumentException("Missing parameter index after '['"); }
					var result = new Variable(text.Substring(pos, end - pos).Trim());
					pos = end + 1;
					SkipWhiteSpace();
					return result;
				}

				const string decimalRegEx = @"(\d+\.?\d*|\d*\.?\d+)";
				var match = Regex.Match(text.Substring(pos), decimalRegEx);
				if (match.Success) {
					pos += match.Length;
					SkipWhiteSpace();
					return new Constant(match.Value);
				} else {
					throw new ArgumentException(string.Format("Unexpeted character '{0}'", c));
				}
			}

			private IExpression ParseTerm()
			{
				var left = ParseFactor();

				while (true) {
					if (pos >= text.Length) {
						return left;
					}

					var c = text[pos];

					if (c == '*' || c == '/') {
						++pos;
						var right = ParseFactor();
						left = new BinaryOperation(c, left, right);
					} else {
						return left;
					}
				}
			}

			private void Require(char c)
			{
				if (pos >= text.Length || text[pos] != c) {
					throw new ArgumentException("Expected '" + c + "'");
				}

				++pos;
			}

			private void RequireEndOfText()
			{
				if (pos != text.Length) {
					throw new ArgumentException("Unexpected character '" + text[pos] + "'");
				}
			}

			private void SkipWhiteSpace()
			{
				while (pos < text.Length && char.IsWhiteSpace((text[pos]))) {
					++pos;
				}
			}

			#endregion Private Methods
		}

		private class Variable : IExpression {

			#region Private Fields

			private readonly int _index;

			#endregion Private Fields

			#region Public Constructors

			public Variable(string text)
			{
				if (!int.TryParse(text, out _index) || _index < 0) {
					throw new ArgumentException(string.Format("'{0}' is not a valid parameter index", text));
				}
			}

			public Variable(int n)
			{
				_index = n;
			}

			#endregion Public Constructors

			#region Public Methods

			public decimal Eval(object[] args)
			{
				if (_index >= args.Length) {
					throw new ArgumentException(string.Format("MathConverter: parameter index {0} is out of range. {1} parameter(s) supplied", _index, args.Length));
				}

				return System.Convert.ToDecimal(args[_index]);
			}

			#endregion Public Methods
		}

		#endregion Private Classes

		#region Private Fields

		private static readonly MathConverter instance = new MathConverter();
		private readonly Dictionary<string, IExpression> _storedExpressions = new Dictionary<string, IExpression>();

		#endregion Private Fields

		#region Public Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(new object[] { value }, targetType, parameter, culture);
		}

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			try {
				var result = Parse(parameter.ToString()).Eval(values);
				if (targetType == typeof(decimal)) {
					return result;
				}

				if (targetType == typeof(string)) {
					return result.ToString();
				}

				if (targetType == typeof(int)) {
					return (int)result;
				}

				if (targetType == typeof(double)) {
					return (double)result;
				}

				if (targetType == typeof(long)) {
					return (long)result;
				}

				if (targetType == typeof(double?)) {
					return (double?)result;
				}

				throw new ArgumentException(string.Format("Unsupported target type {0}", targetType.FullName));
			} catch (Exception ex) {
				ProcessException(ex);
			}

			return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return instance;
		}

		#endregion Public Methods

		#region Protected Methods

		protected virtual void ProcessException(Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		#endregion Protected Methods

		#region Private Methods

		private IExpression Parse(string s)
		{
			if (!_storedExpressions.TryGetValue(s, out var result)) {
				result = new Parser().Parse(s);
				_storedExpressions[s] = result;
			}

			return result;
		}

		#endregion Private Methods
	}
}