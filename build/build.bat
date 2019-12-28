@ECHO OFF
SET Arbor.X.Build.Bootstrapper.AllowPrerelease=true
SET Arbor.X.Tools.External.MSpec.Enabled=true
SET Arbor.X.NuGet.Package.Artifacts.Suffix=
SET Arbor.X.NuGet.Package.Artifacts.BuildNumber.Enabled=
SET Arbor.X.NuGetPackageVersion=
SET Arbor.X.Vcs.Branch.Name.Version.OverrideEnabled=true
SET Arbor.Build.Vcs.Branch.Name=%GITHUB_REF%
SET Arbor.X.Build.VariableOverrideEnabled=true
SET Arbor.X.Artifacts.CleanupBeforeBuildEnabled=true
SET Arbor.X.Build.NetAssembly.Configuration=
SET Arbor.X.MSBuild.NuGetRestore.Enabled=true
SET Arbor.X.Tools.External.Xunit.NetCoreApp.Enabled=false
SET Arbor.Build.BuilderNumber.UnixEpochSecondsEnabled=true

SET Fallback.Version.Build=0

IF "%Arbor.X.Build.Bootstrapper.AllowPrerelease%" == "" (
	SET Arbor.X.Build.Bootstrapper.AllowPrerelease=true
)

SET Arbor.X.NuGet.ReinstallArborPackageEnabled=true
SET Arbor.X.NuGet.VersionUpdateEnabled=false
SET Arbor.X.Artifacts.PdbArtifacts.Enabled=true
SET Arbor.X.NuGet.Package.CreateNuGetWebPackages.Enabled=true

SET Arbor.X.Build.NetAssembly.MetadataEnabled=true
SET Arbor.X.Build.NetAssembly.Description=Milou Deployer Web
SET Arbor.X.Build.NetAssembly.Company=Milou Communication AB
SET Arbor.X.Build.NetAssembly.Copyright=(C) Milou Communication AB 2015-2018
SET Arbor.X.Build.NetAssembly.Trademark=Milou Deployer Web
SET Arbor.X.Build.NetAssembly.Product=Milou Deployer Web
SET Arbor.X.ShowAvailableVariablesEnabled=false
SET Arbor.X.ShowDefinedVariablesEnabled=false
SET Arbor.X.Tools.External.MSBuild.Verbosity=minimal
SET Arbor.X.NuGet.Package.AllowManifestReWriteEnabled=false

CALL dotnet arbor-build

EXIT /B %ERRORLEVEL%