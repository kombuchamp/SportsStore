# SportsStore

This learning project is my implementation of online store.
It has following features:
* Admin page
* Ability to manage products
* Authentication
* User cart

It is implemented with help of Adam Friman's ["Pro ASP.NET Core MVC 2"](https://www.apress.com/us/book/9781484231494) book and built using asp.net core 2 for backend and bootstrap for frontend. SQL Server is used to store product and identity data, Entity Framework Core manages it.

![image](https://user-images.githubusercontent.com/32342483/56087316-3a10b680-5e71-11e9-86ef-333ed49801d9.png)

## How to run
To run this application you need dotnet core and SQL Server installed.
To perform database migration, run:
~~~~
dotnet ef database update --context ApplicationDbContext
dotnet ef database update --context AppIdentityDbContext
~~~~
