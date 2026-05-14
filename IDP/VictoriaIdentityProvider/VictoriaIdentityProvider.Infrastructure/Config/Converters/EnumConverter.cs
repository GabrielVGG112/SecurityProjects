using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace VictoriaIdentityProvider.Infrastructure.Config.Converters
{
    public class EnumConverter<T> : ValueConverter<T, string>
        where T : struct, Enum
    {
        public EnumConverter()
            : base(
                v => v.ToString(),
                v => Parse(v))
        {
        }

        private static T Parse(string value)
        {
            if (Enum.TryParse<T>(value, out var result))
                return result;

            throw new InvalidOperationException(
                $"Cannot convert '{value}' to enum {typeof(T).Name}");
        }
    }
}
