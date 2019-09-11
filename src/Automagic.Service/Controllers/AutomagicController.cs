using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Automagic.Core;
using Automagic.Service.Model;

namespace Automagic.Service.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class AutomagicController : Controller
    {
        /// <summary>
        /// Automagic service description
        /// </summary>
        /// <returns>Service description</returns>
        /// <response code="200">Returns the ServiceDesription</response>
        /// <response code="400">If the item is null</response>            
        [ProducesResponseType(typeof(ServiceDescription), 200)]
        [HttpGet]
        public ServiceDescription Get()
        {
            return new ServiceDescription();
        }

        /// <summary>
        /// Returns a list of the projects managed by this service instance.
        /// </summary>
        /// <returns>The projects.</returns>
        /// <response code="200">Returns the list of projects</response>
        [ProducesResponseType(typeof(List<Project>), 200)]
        [HttpGet("projects")]
        public IActionResult GetProjects()
        {
            return new OkObjectResult(ProjectManager.Instance.Projects);
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <returns>The new project.</returns>
        /// <param name="project">Project.</param>
        [ProducesResponseType(typeof(Project), 201)]
        [HttpPost("projects")]
        public IActionResult CreateProject([FromBody] Project project)
        {
            var pm = ProjectManager.Instance;
            pm.AddProject(project);
            return new OkObjectResult(project);
        }

        [HttpGet("projects/{projectname}")]
        public IActionResult GetProject(string projectname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            if (project == null) return new NotFoundResult();
            return new OkObjectResult(project);
        }

        [HttpGet("projects/{projectname}/sesamconfig")]
        public IActionResult GetSesamConfig(string projectname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            if (project == null) return new NotFoundResult();
            return new OkObjectResult(project);
        }

        [HttpPost("projects/{projectname}/systems")]
        public IActionResult CreateSystem(string projectname, [FromBody] Automagic.Core.System system)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            project.Systems.Add(system);
            return new OkObjectResult(system);
        }

        [HttpGet("projects/{projectname}/systems/{systemname}")]
        public IActionResult GetSystem(string projectname, string systemname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            var system = project.Systems.FirstOrDefault(s => s.Name == systemname);
            return new OkObjectResult(system);
        }

        [HttpGet("projects/{projectname}/systems/{systemname}/model")]
        public IActionResult GetModel(string projectname, string systemname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            var system = project.Systems.FirstOrDefault(s => s.Name == systemname);
            return new OkObjectResult(system.Model);
        }

        [HttpGet("projects/{projectname}/systems/{systemname}/piicandidates")]
        public IActionResult GetPiiCandidates(string projectname, string systemname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            var system = project.Systems.FirstOrDefault(s => s.Name == systemname);
            return new OkObjectResult(system.Model);
        }

        [HttpGet("projects/{projectname}/systems/{systemname}/globalcandidates")]
        public IActionResult GetGlobalCandidates(string projectname, string systemname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            var system = project.Systems.FirstOrDefault(s => s.Name == systemname);
            return new OkObjectResult(system.Model);
        }

        // get globals
        [HttpGet("projects/{projectname}/globals")]
        public IActionResult GetGlobals(string projectname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            return new OkObjectResult(project.Globals);
        }

        [HttpGet("projects/{projectname}/globals/{globalname}")]
        public IActionResult GetGlobal(string projectname, string globalname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            if (project == null) return new NotFoundResult();

            var global = project.Globals.FirstOrDefault(g => g.Name == globalname);
            if (global == null) return new NotFoundResult();

            return new OkObjectResult(global);
        }

        [HttpPut("projects/{projectname}/globals/{globalname}")]
        public IActionResult UpdateGlobal(string projectname, string globalname, [FromBody] Global global)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            var existingGlobal = project.Globals.FirstOrDefault(g => g.Name == globalname);
            if (global == null) return new NotFoundResult();

            if (global.Name != existingGlobal.Name) {
                return new BadRequestResult();    
            }

            project.Globals.Remove(existingGlobal);
            project.Globals.Add(global);

            return new OkResult();
        }

        [HttpGet("projects/{projectname}/incoming")]
        public IActionResult GetIncoming(string projectname, string systemname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            var system = project.Systems.FirstOrDefault(s => s.Name == systemname);
            return new OkObjectResult(system.Model);
        }

        [HttpPost("projects/{projectname}/incoming")]
        public IActionResult AddIncoming(string projectname, string systemname)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            var system = project.Systems.FirstOrDefault(s => s.Name == systemname);
            return new OkObjectResult(system.Model);
        }

        [HttpDelete("projects/{projectname}/incoming/{entitytype}")]
        public IActionResult DeleteIncoming(string projectname, string systemname, string entitytype)
        {
            var project = ProjectManager.Instance.Projects.FirstOrDefault(p => p.Name == projectname);
            var system = project.Systems.FirstOrDefault(s => s.Name == systemname);
            return new OkObjectResult(system.Model);
        }


    }
}
