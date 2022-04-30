function FileSaveAs(filename, filecontent)
{
    var link = document.createElement('a');
    link.download = filename; 
    link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(filecontent);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}