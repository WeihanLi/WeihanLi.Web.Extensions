var solutionPath = "./WeihanLi.Web.Extensions.slnx";
string[] srcProjects = [ 
    "./src/WeihanLi.Web.Extensions/WeihanLi.Web.Extensions.csproj"
];

await DotNetPackageBuildProcess
    .Create(options => 
    {
        options.SolutionPath = solutionPath;
        options.SrcProjects = srcProjects;
    })
    .ExecuteAsync(args);
