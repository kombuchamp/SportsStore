# SportsStore

This learning project is my implementation of online store.
It has following features:
* Admin page
* Ability to manage products
* Authentication
* User cart

It is implemented with help of Adam Friman's ["Pro ASP.NET Core MVC 2"](https://www.apress.com/us/book/9781484231494) book and built using asp.net core 2 for backend and bootstrap for frontend. SQL Server is used to store product and identity data, Entity Framework Core manages it.

![image](https://user-images.githubusercontent.com/32342483/56087308-06359100-5e71-11e9-8c30-a917112a4623.png)

## How to run
To run this application you need dotnet core and SQL Server installed.
To perform database migration, run:
~~~~
dotnet ef database update --context ApplicationDbContext
dotnet ef database update --context AppIdentityDbContext
~~~~
