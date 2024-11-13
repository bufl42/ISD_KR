using Build_BuildersIS.DataBase;
using Build_BuildersIS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build_BuildersIS.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<Project> Projects { get; set; }

        public MainViewModel()
        {
            Projects = new ObservableCollection<Project>();
            LoadProjects();
        }

        private void LoadProjects()
        {
            string query = "SELECT project_id, name, description, start_date, end_date FROM Project";

            DataTable projectData = DatabaseHelper.ExecuteQuery(query);

            foreach (DataRow row in projectData.Rows)
            {
                Projects.Add(new Project
                {
                    ProjectID = Convert.ToInt32(row["project_id"]),
                    ProjectName = row["name"].ToString(),
                    ProjectDescription = row["description"].ToString(),
                    StartDate = Convert.ToDateTime(row["start_date"]),
                    EndDate = Convert.ToDateTime(row["end_date"]),
                    ProjectImage = null // Пока нет изображения, оставим null
                });
            }
        }
    }
}
