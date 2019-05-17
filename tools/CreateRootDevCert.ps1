. .\New-SelfsignedCertificateEx.ps1

New-SelfsignedCertificateEx -Subject "CN=*.deploy.local" -ProviderName "Microsoft Software Key Storage Provider" -Exportable -Path star.deploy.local.pfx -SubjectAlternativeName *.deploy.local,deploy.local -Password (ConvertTo-SecureString "1234" -AsPlainText -Force) -NotAfter $([DateTime]::Now.AddYears(10)) | Out-File DevRootCert.txt