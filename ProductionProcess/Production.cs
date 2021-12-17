using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;
using System.Diagnostics;

namespace ProductionProcess
{
    class Production
    {
        private static Stopwatch stopWatch = new Stopwatch();
        private static List<Product> products;
        private static List<Recipe> recipes;
        private static readonly object locker = new object();
        private static AutoResetEvent isFinished = new AutoResetEvent(false);
        private static AutoResetEvent newResource = new AutoResetEvent(false);
        private static int creationInProcess = 0;

        static void Main(string[] args)
        {
            string filename = "input2.yaml";
            YamlContent output = null;
            try
            {
                output = YamlContent.ParseYaml(filename);
                if(output == null)
                {
                    throw new Exception("Input file is empty");
                }
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                LogError(ex.ToString(), "An error occured while parsing YAML file. Check Log file for deatils.");
                return;
            }
            catch (FileNotFoundException ex)
            {
                LogError(ex.ToString(), "An error occured while opening file. Check Log file for deatils.");
                return;
            }
            catch (Exception ex)
            {
                LogError(ex.ToString(), "File is empty. Nothing to read.");
                return;
            }

            products = output.Products;
            recipes = output.Recipes;

            // If there was additional products in some recipes, we should add them to common product list
            CheckForLostProducts();

            //Adding recipes references to Projects
            List<Project> projects = new List<Project>();
            foreach (Project project in output.Projects)
            {
                foreach (Abilitie abilitie in project.Abilities)
                {
                    Recipe tmp = recipes.Where(rec => rec.Name == abilitie.Name).First();
                    abilitie.Recipe = tmp;
                }
                projects.Add(project);
            }

            //Adding projects references to Buildings
            List<Building> buildings = new List<Building>();
            foreach (Building building in output.Buildings)
            {
                Project tmp = projects.Where(proj => proj.Name == building.ProjectName).First();
                building.Project = tmp;
                buildings.Add(building);
            }

            Console.WriteLine("productionStartLog:");
            stopWatch.Start();
            LoadAllFactorys(buildings);
            Console.WriteLine($"Total time: {GetTime()}");
            Console.WriteLine("products:");
            foreach (Product product in products)
            {
                if (product.Quantity > 0)
                {
                    Console.Write($"{product.Name} = {product.Quantity};");
                }
                //Console.Write($"{product.Name} = {product.Quantity};");
            }
            Console.WriteLine("\nPress any key to quit");
            Console.ReadKey();
        }
        private static void CheckForLostProducts()
        {
            foreach (Recipe recipe in recipes)
            {
                List<string> existingProducts = products.Select(x => x.Name).ToList();
                List<string> lostProducts = recipe.Components.Where(product => !existingProducts.Contains(product.Name)).Select(x => x.Name).ToList();
                if (!existingProducts.Contains(recipe.Product.Name))
                {
                    lostProducts.Add(recipe.Product.Name);
                }
                if (lostProducts.Count != 0)
                {
                    foreach (string name in lostProducts)
                    {
                        products.Add(new Product(name, 0));
                    }
                }
            }
        }

        private static bool IsCreatable(Recipe recipe)
        {
            lock (locker)
            {
                bool creatable = true;
                recipe.Components.ForEach(component =>
                {
                    if (products.Where(x => x.Name == component.Name && x.Quantity >= component.Quantity)
                    .Count() == 0)
                    {
                        creatable = false;
                    }
                });
                return creatable;
            }
        }

        private static bool IsWorkable()
        {
            foreach (Recipe recipe in recipes)
            {
                if (IsCreatable(recipe))
                {
                    return true;
                }
            }
            if(creationInProcess > 0)
            {
                return true;
            }
            return false;
        }

        private static void CreateProduct(Abilitie abilitie, Building building)
        {
            creationInProcess++;
            newResource.Reset();
            Console.WriteLine($"time: {GetTime()}, factory: {building.Name}, recipe: {abilitie.Recipe.Name}");
            Thread.Sleep(1000 * abilitie.Duration);
            PutResource(abilitie.Recipe);
            newResource.Set();
            creationInProcess--;
        }

        private static void FactoryStart(Building building)
        {
            while (IsWorkable()){  
                for (int i = 0; i < building.Project.Abilities.Count(); i++)
                {
                    Abilitie abilitie = building.Project.Abilities[i];
                    if (GetResources(abilitie.Recipe))
                    {
                        CreateProduct(abilitie, building);
                        break;
                    }
                }
                newResource.WaitOne();
                newResource.Set();
            }
            isFinished.Set();
        }

        private static void LogError(string log, string message)
        {
            Console.WriteLine(message);
            using (StreamWriter writer = File.AppendText("log.txt"))
                writer.WriteLine($"{DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss")}\n {log} \n");
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }

        private static void LoadAllFactorys(List <Building> buildings)
        {
            foreach (Building factory in buildings)
            {
                Thread thread = new Thread(() => FactoryStart(factory));
                thread.Name = factory.Name;
                thread.Start();
            }
            isFinished.WaitOne();
        }

        private static bool GetResources(Recipe recipe)
        {
            if (!IsCreatable(recipe))
            {
                return false;
            }
            lock (locker)
            {
                foreach (Product component in recipe.Components)
                {
                    products.Where(x => x.Name == component.Name).First().Quantity -= component.Quantity;
                }
                return true;
            }
        }

        private static void PutResource(Recipe recipe)
        {
            lock (locker)
            {
                products.Find(x => x.Name == recipe.Product.Name).Quantity += recipe.Product.Quantity;
            }
        }

        private static int GetTime()
        {
            return stopWatch.Elapsed.Seconds + stopWatch.Elapsed.Minutes * 60 + stopWatch.Elapsed.Hours * 3600;
        }
    }
}
