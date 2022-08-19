using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace DirectoryComparer.Services
{
    public class ImagesManager
    {
        public static ImageList GetImages()
        {
            var manager = new ResourceManager(typeof(DirectoryComparerIcons));
            var imageList = new ImageList();

            var folder = (Image)manager.GetObject("folder");
            imageList.Images.Add(folder);

            return imageList;
        }
    }
}