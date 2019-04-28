# index

This service is asp.net core web app

## Vulnerabilities
### First vuln — 'not production' backdoor
There is `IsAdminSession` method at `NotesController.cs` line 55. It checks `!hostingEnvironment.IsProduction()` which can be specified in the environment variable `ASPNETCORE_ENVIRONMENT` and `admin` cookie existence.

#### Attack
Make GET request to `api/notes?isPublic=false` with non empty `admin` cookie.


[Exploit](admin_coockie_exploit.py)
 
#### Defence
Just restart service with `ASPNETCORE_ENVIRONMENT=Production`

### Second vuln — zip path traversal
Service saves data in filesystem-like collection in mongodb.
User can add zip archives and list directories content by searching files using it's name.

#### Attack
You can build up special zip containing file with name like '../../payload'
Expression at 58 string in `IndexHelper.cs` will collapse '..' and 'payload' will be at the root directory.

Then you can search this file 'payload' and list all files in root directory.
After just walk around with some traversing algorithm like [BFS](https://en.wikipedia.org/wiki/Breadth-first_search) and search for the filenames that satisfies flag pattern.

It is helpful to cache results between exploit runs — you can save some requests

[Exploit](zip_path_exploit.py)
 
#### Defence
There are some options:
- write proxy that blocks zips with path traversal
- decompile, fix logic and rebuild service by yourself 
 