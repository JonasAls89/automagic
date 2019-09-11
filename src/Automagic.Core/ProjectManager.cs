using System;
using System.Collections.Generic;

namespace Automagic.Core
{
    /// <summary>
    /// Project manager. A singleton container for the project objects.
    /// </summary>
    public class ProjectManager
    {
        /// <summary>
        /// The singleton instance that is used as the access point. 
        /// Could be replaced with DI.
        /// </summary>
        private static ProjectManager _instance;

        /// <summary>
        /// Gets the ProjectManager instance.
        /// </summary>
        /// <value>The project manager instance.</value>
        public static ProjectManager Instance {
            get {
                if (_instance == null) {
                    _instance = new ProjectManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the projects.
        /// </summary>
        /// <value>The projects.</value>
        public List<Project> Projects { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Automagic.Core.ProjectManager"/> class.
        /// </summary>
        public ProjectManager()
        {
            Projects = new List<Project>();
        }

        /// <summary>
        /// Adds the specified project.
        /// </summary>
        /// <param name="project">The new project to add.</param>
        public void AddProject(Project project) {
            Projects.Add(project);
        }
    }
}
