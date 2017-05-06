﻿using CodingDocs.Models;
using CodingDocs.Models.Entities;
using CodingDocs.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodingDocs.Services
{
    public class ProjectService
    {
        private ApplicationDbContext _db;

        public ProjectService()
        {
            _db = new ApplicationDbContext();
        }

        public List<ProjectViewModel> GetIndividualProjects(string userId)
        {
            var projects = (from proj in _db.Projects
                            where proj.OwnerID == userId
                            select proj)
                            .Select(x => new ProjectViewModel { ID = x.ID, Name = x.Name, Type = x.Type })
                            .ToList();
            return projects;
        }

        public List<ProjectViewModel> GetSharedProjects(string userId)
        {
            var projects = (from proj in _db.Projects
                            join uip in _db.UsersInProjects on proj.ID equals uip.ProjectID
                            where uip.UserID == userId
                            select proj)
                            .Select(x => new ProjectViewModel { ID = x.ID, Name = x.Name, Type = x.Type })
                            .ToList();
            return projects;
        }

        // Maybe return bool?
        public void CreateProject(CreateProjectViewModel projectVM)
        {
            Project project = new Project
            {
                Name = projectVM.Name,
                Type = projectVM.Type,
                OwnerID = projectVM.OwnerID,
            };

            _db.Projects.Add(project);
            _db.SaveChanges();

            CreateFileViewModel file = new CreateFileViewModel
            {
                Name = "index",
                Type = project.Type,
                ProjectID = project.ID
            };

            CreateFile(file);
        }

        public void ShareProject(ShareProjectViewModel projectVM)
        {
            var userID = (from usr in _db.Users
                        where usr.UserName == projectVM.UserName
                        select usr.Id)
                        .SingleOrDefault();

            UsersInProject uip = new UsersInProject
            {
                ProjectID = projectVM.ProjectID,
                UserID = userID
            };

            _db.UsersInProjects.Add(uip);
            _db.SaveChanges();
        }

        public bool ValidUserName(string userName)
        {
            var user = (from usr in _db.Users
                        where usr.UserName == userName
                        select usr)
                        .SingleOrDefault();

            if (user == null) return false;

            return true;
   
        }

        public ViewProjectViewModel GetProject(int projectId)
        {
            var project = (from proj in _db.Projects
                           where proj.ID == projectId
                           select proj)
                           .Select(x => new ViewProjectViewModel { ID = x.ID, Name = x.Name, Type = x.Type })
                           .SingleOrDefault();

            var files = (from file in _db.Files
                         join fip in _db.FilesInProjects on file.ID equals fip.FileID
                         where fip.ProjectID == projectId
                         select file)
                         .Select(x => new FileViewModel { ID = x.ID, Name = x.Name, Type = x.Type, Content = x.Content })
                         .ToList();

            project.Files = files;

            return project;
        }

        public bool AuthorizeProject(string userId, int projectId)
        {
            var project = (from proj in _db.Projects
                           where proj.ID == projectId
                           select proj).SingleOrDefault();

            if(project == null) return false;

            if(project.OwnerID == userId) return true;

            var sharedProject = (from uip in _db.UsersInProjects
                                 where uip.ProjectID == projectId
                                 && uip.UserID == userId
                                 select uip).SingleOrDefault();

            if(sharedProject != null) return true;    

            return false;
        }

        public void CreateFile(CreateFileViewModel fileVM)
        {
            File file = new File
            {
                Name = fileVM.Name,
                Type = fileVM.Type,
                Content = "",
            };

            _db.Files.Add(file);
            _db.SaveChanges();

            FilesInProject fip = new FilesInProject
            {
                FileID = file.ID,
                ProjectID = fileVM.ProjectID
            };

            _db.FilesInProjects.Add(fip);
            _db.SaveChanges();
        }
    }
}