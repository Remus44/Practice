Környezet:
- .net8 SDK
  https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- IDE: Visual Studio Code, Visual Studio vagy Jetbrains Rider
	Visual Studio Code esetén lehet kell az alábbi plugin: C# Dev Kit for Visual Studio Code
- dotnet build
- dotnet run
- docker MySQL
  docker run --name mysql-db -e MYSQL_ROOT_PASSWORD=root -d -p 3306:3306 mysql:9.5
- SQL server is lehet, de akkor kell külön library és más a connection string
- db-ben nem kell módosítani semmit, migrációt kell használni helyette
- nagy adatmennyiségben gondolkodjunk mindenhol, használjunk indexeket is
- nem kell git verziókezelés
- vannak sebezhető függőségek, ezekkel most nem kell foglalkozni
- Az UML-ben long van, a kódban int. Nagy adatmennyiségben kell gondolkodni, így átírhatod, ha ezek alapján indokoltnak tartod. 
- unit tesztek nem kellenek, nem lesz rá idő  
- a for ciklust hanyagoljuk ha lehet, inkább LINQ-t preferáljuk

SQL generálás:
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 8.0.10
dotnet ef migrations add InitialCreate
dotnet ef database update

Teszt Poweshellben:
Invoke-WebRequest -Uri "http://localhost:5045/api/User/createUser" -Method POST -ContentType "application/json" -Body '{"Name":"John Doe","Email":"john.doe@example.com","RoleCodes":["ADMIN","USER"]}'

postman, soapui