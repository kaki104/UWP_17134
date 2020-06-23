using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadFileSystemAccess
{
    /// <summary>
    /// 폴더 모델
    /// </summary>
    public class FolderModel : ObservableObject
    {
        /// <summary>
        /// 이름
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 패스
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 서브디렉토리 존재 여부
        /// </summary>
        public bool HasSubDirectory
        {
            get { return SubFolders != null && SubFolders.Any(); }
        }

        private IList<FolderModel> subFolders;
        /// <summary>
        /// SubFolders
        /// </summary>
        public IList<FolderModel> SubFolders
        {
            get => subFolders;
            set
            {
                SetProperty(ref subFolders, value);
                OnPropertyChanged(nameof(HasSubDirectory));
            }
        }

        private bool isExpaneded;
        /// <summary>
        /// IsExpaneded
        /// </summary>
        public bool IsExpanded
        { 
            get => isExpaneded; 
            set => SetProperty(ref isExpaneded ,value); 
        }
    }
}
