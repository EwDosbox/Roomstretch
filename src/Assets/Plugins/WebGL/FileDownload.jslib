mergeInto(LibraryManager.library, {
    DownloadFile: function(data, filename) {
        // Convert Unity strings to JavaScript strings
        const textData = UTF8ToString(data);
        const fileName = UTF8ToString(filename);

        // Create Blob and download link
        const blob = new Blob([textData], { type: 'text/xml' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
});