using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2
{
    /// <summary>
    /// 디렉토리 모델
    /// </summary>
    public class DirectoryModel
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
        public bool HasSubDirectory { get; set; }

        public bool IsExpaned { get; set; }
    }
}
