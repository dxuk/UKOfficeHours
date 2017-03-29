REM Spin up storage emulator
AzureStorageEmulator START

cd 

cmd /C "C:\Program Files (x86)\IIS Express\IISEXPRESS /path:C:\Users\wieastbu\Source\Repos\UKOfficeHours\UKOfficeHours\wwwroot"

REM SET ENV VARS
SET AzureWebJobsstorageConnection=UseDevelopmentStorage=true
SET AzureStorageConnection=UseDevelopmentStorage=true
SET storageConnection=UseDevelopmentStorage=true
SET AzureWebJobsStorage=UseDevelopmentStorage=true
SET AzureWebJobsDashboard=UseDevelopmentStorage=true

CMD /C "func host start"