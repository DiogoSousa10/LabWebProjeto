# labwebprojeto  
dotnet user-secrets set SendGridKey <key>  

**Enable user secrets**:  
dotnet user-secrets init -p labwebprojeto (todos os comandos tem de levar o -p labwebprojeto)

**Google API Settings**:  
dotnet user-secrets set "Authentication:Google:ClientId" "**"  
dotnet user-secrets set "Authentication:Google:ClientSecret" "**"   

**Microsoft API Settings**:  
dotnet user-secrets set "Authentication:Microsoft:ClientId" "**"  
dotnet user-secrets set "Authentication:Microsoft:ClientSecret" "**"

**Twitter API Settings**: 
dotnet user-secrets set "Authentication:Twitter:ConsumerAPIKey" "**"  
dotnet user-secrets set "Authentication:Twitter:ConsumerSecret" "**"  
Bearer Token:**

**Cloudinary API Settings**:   
Check JsonFile

**Para o Toastr**:   
https://github.com/CodeSeven/toastr  
