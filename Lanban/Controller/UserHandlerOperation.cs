﻿using Lanban.AccessLayer;
using Lanban.Model;
using System;
using System.Threading;
using System.Web;

namespace Lanban
{
    public class UserHandlerOperation : HandlerOperation
    {
        UserAccess myAccess;

        public UserHandlerOperation(AsyncCallback callback, HttpContext context, Object state)
            :base(callback, context, state)
        {
            myAccess = new UserAccess();
        }

        public void QueueWork()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartTask), null);
        }

        private void StartTask(Object workItemState)
        {
            var param = _context.Request.Params;
            int projectID = 0, userID = 0, role = 0;
            var user = new UserModel();

            // Check whether a username is taken - only for login page
            if (action.Equals("checkUsername"))
                result = myAccess.checkUsername(param["username"]);
            else
            {
                projectID = Convert.ToInt32(param["projectID"]);
                user = (UserModel)_context.Session["user"];
            }

            // Proceed other actions as usual
            try
            {
                bool error = false;
                userID = user.User_ID;
                role = user.Role;
                switch (action)
                {
                    /***********************************************/
                    // Search name of members in a project
                    case "searchAssignee":
                        result = myAccess.searchAssignee(projectID, param["keyword"], param["type"]);
                        break;

                    // View all assignees/members of an item - Backlog/Task
                    case "viewAssignee":
                        result = myAccess.viewAssignee(param["itemID"], param["type"]);
                        break;

                    // Save assignee/member of an object - Project/Backlog/Task
                    case "saveAssignee":
                        string aID = param["assigneeID"];
                        myAccess.saveAssignee(param["itemID"], param["type"], aID);
                        break;

                    // Delete all assignees/member of an object - Backlog/Task
                    case "deleteAssignee":
                        myAccess.deleteAssignee(param["itemID"], param["type"]);
                        break;

                    // A user quit a project
                    case "quitProject":
                        if (!myAccess.quitProject(projectID, userID, role)) error = true;
                        break;

                    // Get the user data based on name and role
                    case "searchUser":
                        result = myAccess.searchUser(param["name"], Convert.ToInt32(param["role"]));
                        break;

                    /***********************************************/
                    // Working with supervisor
                    // Delete all supervisor of a project
                    case "deleteSupervisor":
                        myAccess.deleteSupervisor(projectID);
                        break;

                    // Save supervisor of a project
                    case "saveSupervisor":
                        myAccess.saveSupervisor(projectID, Convert.ToInt32(param["supervisorID"]));
                        break;
                }

                if (error) Code = 500;
            }
            catch
            {
                Code = 500;
            }
            finally
            {
                myAccess.Dipose();
                FinishWork();
            }
        }
    }
}