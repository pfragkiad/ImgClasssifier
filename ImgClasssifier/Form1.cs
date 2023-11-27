using ImgClasssifier.Images;
using ImgClasssifier.Rating;
using Microsoft.Extensions.DependencyInjection;

namespace ImgClasssifier;

//TODO: Remove rated photo.
//TODO: Change rating.
//TODO: Reclassify (change all ratings) -> 
//TODO: Reclassify (compare mode).

public partial class Form1 : Form
{
    public Form1(
        PictureRater rater)
    {
        InitializeComponent();

        _rater = rater;

        AddRaterEvents();

        toolTip1.SetToolTip(trackBar1, $"{trackBar1.Value}");

        _rater.LoadUnratedFilesAndCheckRatedFiles();
    }

    private void AddRaterEvents()
    {
        _rater.LoadingUnratedFiles += (o, e) => txtLog.Clear();

        _rater.RemovedDuplicateAlreadyRatedFile +=
          (o, e) =>
          {
              txtLog.AppendText(
                  e.Success ?
                  $"Removed {e.RemovedFilename}: exists as {e.RatedFilename}\r\n" :
                  $"Could not remove {e.RemovedFilename}: exists as {e.RatedFilename}\r\n")
                  ;
          };

        _rater.ResetCompleted +=
            (o, e) =>
            {
                Text = "<No files>";
                pictureBox1.Image = null;
                pictureBox1.ImageLocation = "";
            };

        _rater.ProceededToNextFile +=
            (o, e) =>
            {
                Text = $"{_rater.CurrentFile}-[{_rater.CurrentIndex + 1}/{_rater.UnratedImagesCount}]";
                LoadTransformedImage();
                //pictureBox1.ImageLocation = _rater.CurrentFile;
            };

        _rater.MoveImageFailed +=
            (o, e) =>
            {
                txtLog.AppendText($"Could not move {e.UnratedFilename}: {e.Reason}\r\n");
            };

    }

    private void LoadTransformedImage()
    {
        if (_rater.CurrentFile == "") return;

        pictureBox1.Image = ImageExtensions.GetUnlockedImageFromFile(_rater.CurrentFile, 
            chkRotateRight.Checked ? RotateFlipType.Rotate90FlipNone : RotateFlipType.RotateNoneFlipNone);
    }


    private readonly PictureRater _rater;


    private void btnRefresh_Click(object sender, EventArgs e)
    {
        _rater.LoadUnratedFilesAndCheckRatedFiles();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (_rater.CurrentFile == "") return;

        _rater.SaveCurrentFile(trackBar1.Value);
        _rater.GotoNextFile();
    }




    private void trackBar1_Scroll(object sender, EventArgs e)
    {
        toolTip1.SetToolTip(trackBar1, $"{trackBar1.Value}");
    }

    private void btnSkip_Click(object sender, EventArgs e)
    {
        _rater.GotoNextFile();
    }

    private void chkRotateRight_CheckedChanged(object sender, EventArgs e)
    {
        LoadTransformedImage();
    }

    private void btnResetOrderInLogFile_Click(object sender, EventArgs e)
    {
        _rater.ResetSortOrderInLogfile();
        MessageBox.Show("Reset order complete!");
    }

    private void btnBrowseRated_Click(object sender, EventArgs e)
    {
        var form = Program.Provider.GetRequiredService<BrowserForm>();
        Cursor.Current = Cursors.WaitCursor;
        form.UpdateBrowser();
        Cursor.Current = Cursors.Default;
        form.ShowDialog();
    }
}
