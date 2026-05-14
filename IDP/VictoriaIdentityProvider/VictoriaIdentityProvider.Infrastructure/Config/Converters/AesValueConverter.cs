using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Infrastructure.Security;

namespace VictoriaIdentityProvider.Infrastructure.Config.Converters
{
    internal class AesValueConverter : ValueConverter<string ,string>
    {
        public AesValueConverter(ICipher aes) :
            base(v => aes.Encrypt(v),
            v => aes.Decrypt(v))
        {

        }
    }
}
