// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Style",
        "IDE0034:Simplify 'default' expression",
        Justification = "Too make style cop avoid C# 7 errors",
        Scope = "member",
        Target =
            "~M:Milou.Deployer.Web.IisHost.Areas.Deployment.Services.PackageService.GetPackageVersionsAsync(System.String,System.Boolean,Serilog.ILogger,System.Boolean,System.String,System.String,System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.Collections.Generic.IReadOnlyCollection{Milou.Deployer.Web.Core.Deployment.PackageVersion}}")]
[assembly:
    SuppressMessage("Style",
        "IDE0034:Simplify 'default' expression",
        Justification = "Too make style cop avoid C# 7 errors",
        Scope = "member",
        Target =
            "~M:Milou.Deployer.Web.IisHost.Areas.Targets.Controllers.TargetsController.Index(System.Threading.CancellationToken)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Mvc.IActionResult}")]
