using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FunnyDB
{
    public class SqlQueryLinter : IDisposable
    {
        private readonly BinaryReader _reader;
        private int _line = 1;
        private int _position;


        /// <summary>
        /// Validates content in specified stream.
        /// </summary>
        /// <param name="code">A code to validate.</param>
        /// <param name="errors">A collection of errors if found.</param>
        /// <returns>Returns true if content in stream are correct; otherwise returns false.</returns>
        public static bool Validate(string code, out IReadOnlyCollection<Error> errors)
        {
            var bytes = Encoding.UTF8.GetBytes(code);
            using (var ms = new MemoryStream(bytes))
            {
                ms.Seek(0, SeekOrigin.Begin);
                return Validate(ms, out errors);
            }
        }

        /// <summary>
        /// Validates content in specified stream.
        /// </summary>
        /// <param name="stream">A stream with content to validate.</param>
        /// <param name="errors">A collection of errors if found.</param>
        /// <returns>Returns true if content in stream are correct; otherwise returns false.</returns>
        public static bool Validate(Stream stream, out IReadOnlyCollection<Error> errors)
        {
            var linter = new SqlQueryLinter(stream);
            return linter.Validate(out errors);
        }

        public SqlQueryLinter(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public void Dispose()
        {
            _reader.Close();
        }

        private bool Validate(out IReadOnlyCollection<Error> errors)
        {
            var collectedErrors = (List<Error>) null;

            // sql(()=>
            // 01234567
            var lastChars = new char[8];
            var lastCharsPos = 0;
            var isInSqlContext = false;
            while (ReadNext(out var ch))
            {
                if (char.IsWhiteSpace(ch))
                {
                    continue;
                }

                lastChars[lastCharsPos % lastChars.Length] = ch;
                lastCharsPos += 1;

                if (IsSqlContextStart(lastChars, lastCharsPos - 1))
                {
                    if (isInSqlContext)
                    {
                        throw new InvalidOperationException(
                            "Unexpected 'sql(()=>' was found. " +
                            "It looks like a bug in linter, please report to FunnyDB developer.");
                    }

                    isInSqlContext = true;
                    continue;
                }

                if (IsLongString(lastChars, lastCharsPos - 1, out var isInterpolation))
                {
                    if (!ValidateLongString(isInSqlContext && isInterpolation, out var stringErrors))
                    {
                        collectedErrors = collectedErrors ?? new List<Error>();
                        collectedErrors.AddRange(stringErrors);
                        continue;
                    }
                }

                if (IsShortString(lastChars, lastCharsPos - 1, out isInterpolation))
                {
                    if (!ValidateShortString(isInSqlContext && isInterpolation, out var stringErrors))
                    {
                        collectedErrors = collectedErrors ?? new List<Error>();
                        collectedErrors.AddRange(stringErrors);
                        continue;
                    }
                }
            }

            errors = (IReadOnlyCollection<Error>) collectedErrors ?? Array.Empty<Error>();
            return errors.Count == 0;
        }

        private bool ValidateLongString(bool validateInterpolation, out List<Error> errors)
        {
            errors = new List<Error>();
            while (ReadNext(out var ch))
            {
                if (ch == '"' && PeekNext(out var nextCh) && nextCh != '"')
                {
                    return errors.Count == 0;
                }

                if (!validateInterpolation)
                {
                    continue;
                }

                ValidateInterpolation(errors, ch);
            }

            return errors.Count == 0;
        }

        private bool ValidateShortString(bool validateInterpolation, out List<Error> errors)
        {
            errors = new List<Error>();

            char? prevCh = null;
            while (ReadNext(out var ch))
            {
                try
                {
                    if (ch == '"' && prevCh.HasValue && prevCh.Value != '\\')
                    {
                        return errors.Count == 0;
                    }

                    if (!validateInterpolation)
                    {
                        continue;
                    }

                    ValidateInterpolation(errors, ch);
                }
                finally
                {
                    prevCh = ch;
                }
            }

            return errors.Count == 0;
        }

        private void ValidateInterpolation(ICollection<Error> errors, char ch)
        {
            if (ch == '{' && PeekNext(out var nextCh) && nextCh == '{')
            {
                ReadNext(out _);
                return;
            }

            if (ch != '{' || !PeekNext(out nextCh))
            {
                return;
            }

            if (nextCh != 'p')
            {
                errors.Add(new Error(_line, _position));
                return;
            }

            ReadNext(out _);
            if (PeekNext(out nextCh) && nextCh != '(')
            {
                errors.Add(new Error(_line, _position));
            }
        }

        private static bool IsSqlContextStart(char[] lastChars, int pos)
        {
            if (pos < 7)
            {
                return false;
            }
            
            var l = lastChars.Length;
            return lastChars[(pos - 7) % l] == 's'
                   && lastChars[(pos - 6) % l] == 'q'
                   && lastChars[(pos - 5) % l] == 'l'
                   && lastChars[(pos - 4) % l] == '('
                   && lastChars[(pos - 3) % l] == '('
                   && lastChars[(pos - 2) % l] == ')'
                   && lastChars[(pos - 1) % l] == '='
                   && lastChars[(pos - 0) % l] == '>';
        }

        private static bool IsLongString(char[] lastChars, int pos, out bool isInterpolation)
        {
            if (pos < 2)
            {
                isInterpolation = false;
                return false;
            }

            var l = lastChars.Length;
            if (lastChars[(pos - 2) % l] == '$' && lastChars[(pos - 1) % l] == '@' && lastChars[(pos - 0) % l] == '"')
            {
                isInterpolation = true;
                return true;
            }

            if (lastChars[(pos - 2) % l] == '@' && lastChars[(pos - 1) % l] == '$' && lastChars[(pos - 0) % l] == '"')
            {
                isInterpolation = true;
                return true;
            }

            if (lastChars[(pos - 1) % l] == '@' && lastChars[(pos - 0) % l] == '"')
            {
                isInterpolation = false;
                return true;
            }

            isInterpolation = false;
            return false;
        }

        private static bool IsShortString(char[] lastChars, int pos, out bool isInterpolation)
        {
            if (pos < 1)
            {
                isInterpolation = false;
                return false;
            }

            var l = lastChars.Length;
            if (lastChars[(pos - 1) % l] == '$' && lastChars[(pos - 0) % l] == '"')
            {
                isInterpolation = true;
                return true;
            }

            if (lastChars[(pos - 0) % l] == '"')
            {
                isInterpolation = false;
                return true;
            }

            isInterpolation = false;
            return false;
        }


        private bool PeekNext(out char ch)
        {
            var peek = _reader.PeekChar();
            if (peek == -1)
            {
                ch = default;
                return false;
            }

            ch = (char) peek;
            return true;
        }

        private bool ReadNext(out char ch)
        {
            if (_reader.PeekChar() == -1)
            {
                ch = default;
                return false;
            }

            ch = _reader.ReadChar();
            if (ch == '\n')
            {
                _line += 1;
                _position = 0;
            }
            else
            {
                _position += 1;
            }

            return true;
        }

        public class Error
        {
            public Error(int line, int position)
            {
                Line = line;
                Position = position;
            }

            public readonly int Line;
            public readonly int Position;
        }
    }
}