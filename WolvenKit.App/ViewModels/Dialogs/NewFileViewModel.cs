using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using WolvenKit.Common;
using WolvenKit.Common.Model;
using System.Xml.Serialization;
using System.Reflection;
using System.Windows.Input;
using WolvenKit.Functionality.Commands;
using System.IO;
using WolvenKit.Functionality.Services;
using Splat;
using WolvenKit.MVVM.Model.ProjectManagement.Project;
using WolvenKit.ViewModels.Shell;
using System.Reactive.Linq;

namespace WolvenKit.ViewModels.Dialogs
{
    public class NewFileViewModel : DialogViewModel
    {

        public delegate Task ReturnHandler(NewFileViewModel file);
        public ReturnHandler FileHandler;

        public NewFileViewModel()
        {
            CloseCommand = ReactiveCommand.Create(() => { });
            CreateCommand = ReactiveCommand.Create(() =>
            {
                IsCreating = true;
                FileHandler(this);
            }, this.WhenAnyValue(
                x => x.FileName, x => x.FullPath, x => x.IsCreating,
                (file, path, isCreating) =>
                    !isCreating &&
                    file != null &&
                    !string.IsNullOrEmpty(file) &&
                    !File.Exists(path)));
            Cancel2Command = ReactiveCommand.Create(() => FileHandler(null));

            Title = "Create new file";

            try
            {
                var serializer = new XmlSerializer(typeof(WolvenKitFileDefinitions));
                using (var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(@"WolvenKit.App.Resources.WolvenKitFileDefinitions.xml"))
                {
                    var newdef = (WolvenKitFileDefinitions)serializer.Deserialize(stream);
                    Categories = new ObservableCollection<FileCategoryModel>(newdef.Categories);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //CanCreate = Observable.Empty<bool>().StartsWith(false);

            //=> FileName != null && !string.IsNullOrEmpty(FileName) && !File.Exists(FullPath);


            this.WhenAnyValue(x => x.SelectedFile)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        FileName = $"{x.Name.Split(' ').First()}1.{x.Extension.ToLower()}";
                    }
                    else
                    {
                        FileName = null;
                    }
                });
            this.WhenAnyValue(x => x.FileName)
                .Subscribe(x =>
                {
                    if (SelectedFile != null && x != null)
                    {
                        FullPath = Path.Combine(GetDefaultDir(SelectedFile.Type), x);
                        WhyNotCreate = File.Exists(FullPath) ? "Filename already in use" : "";
                    } else
                    {
                        WhyNotCreate = "";
                    }
                });
        }

        [Reactive] public string Text { get; set; }

        [Reactive] public bool IsCreating { get; set; }

        [Reactive] public string FileName { get; set; }
        [Reactive] public string FullPath { get; set; }

        public sealed override string Title { get; set; }

        public sealed override ReactiveCommand<Unit, Unit> CloseCommand { get; set; }
        public sealed override ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
        public sealed override ReactiveCommand<Unit, Unit> OkCommand { get; set; }


        [Reactive] public ObservableCollection<FileCategoryModel> Categories { get; set; } = new();

        [Reactive] public FileCategoryModel SelectedCategory { get; set; }

        [Reactive] public AddFileModel SelectedFile { get; set; }

        public ICommand CreateCommand { get; private set; }
        public ICommand Cancel2Command { get; private set; }
        [Reactive] public string WhyNotCreate { get; set; }
        //private async Task ExecuteCreate()
        //{

        //    //var fullPath = string.IsNullOrEmpty(inputDir)
        //    //? Path.Combine(GetDefaultDir(SelectedFile.Type), filename)
        //    //: Path.Combine(inputDir, filename);

        //    switch (SelectedFile.Type)
        //    {
        //        case EWolvenKitFile.Redscript:
        //        case EWolvenKitFile.Tweak:
        //            if (!string.IsNullOrEmpty(SelectedFile.Template))
        //            {
        //                await using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream($"WolvenKit.App.Resources.{SelectedFile.Template}"))
        //                {
        //                    using var fileStream = new FileStream(FullPath, FileMode.Create, FileAccess.Write);
        //                    resource.CopyTo(fileStream);
        //                }
        //            }
        //            else
        //            {
        //                File.Create(FullPath);
        //            }
        //            break;
        //        case EWolvenKitFile.Cr2w:
        //            //CreateCr2wFile(SelectedFile);
        //            break;
        //    }

        //    // Open file
        //    await Locator.Current.GetService<AppViewModel>().RequestFileOpen(FullPath);
        //}

        private string GetDefaultDir(EWolvenKitFile type)
        {
            var project = Locator.Current.GetService<IProjectManager>().ActiveProject as Cp77Project;
            return type switch
            {
                EWolvenKitFile.Redscript => project.ScriptDirectory,
                EWolvenKitFile.Tweak => project.TweakDirectory,
                EWolvenKitFile.Cr2w => project.ModDirectory,
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }

   

    
}
