using System;
using System.Runtime.Serialization;

namespace Milou.Deployer.Web.Core
{
    public class DeployerAppException : Exception
    {
        public DeployerAppException()
        {
        }

        protected DeployerAppException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DeployerAppException(string message) : base(message)
        {
        }

        public DeployerAppException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}