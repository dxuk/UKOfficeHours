# Deploy the function app #
Remove-Item ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\functions.zip") -ErrorAction ignore
[IO.Compression.ZipFile]::CreateFromDirectory(((Get-Item -Path ".\" -Verbose).FullName + "\functions\"),((Get-Item -Path ".\" -Verbose).FullName + "\deploy\functions.zip"))

# Deploy the front end site #
Remove-Item ((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip") -ErrorAction ignore
[IO.Compression.ZipFile]::CreateFromDirectory(((Get-Item -Path ".\" -Verbose).FullName + "\wwwroot\"),((Get-Item -Path ".\" -Verbose).FullName + "\deploy\web.zip"))


















