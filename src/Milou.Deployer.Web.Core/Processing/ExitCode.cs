using System;

namespace Milou.Deployer.Web.Core.Processing
{
    public struct ExitCode
    {
        public int Code { get; }

        public ExitCode(int code)
        {
            Code = code;
        }

        public static implicit operator int(ExitCode exitCode)
        {
            return exitCode.Code;
        }

        public override string ToString()
        {
            return string.Format("EXIT CODE [{0}] {1}", Code, (IsSuccess ? "Success" : "Failure"));
        }

        public static readonly ExitCode Success = new ExitCode(0);
        public static readonly ExitCode Failure = new ExitCode(1);

        public static ExitCode Failed(int exitCode)
        {
            if (exitCode == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exitCode), "Exit code cannot be 0 when failed");
            }

            return new ExitCode(exitCode);
        }

        public bool IsSuccess => Code == 0;
    }
}