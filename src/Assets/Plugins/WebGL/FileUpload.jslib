mergeInto(LibraryManager.library, 
{
    BrowserTextUpload: function(extFilter, gameObjName, dataSinkFn)
    {
        if(typeof inputLoader == "undefined")
        {
            inputLoader = document.createElement("input");
            inputLoader.setAttribute("type", "file");
            inputLoader.style.display = 'none';
            document.body.appendChild(inputLoader);

            inputLoader.onchange = 
                function(x)
                {
                    if(this.value == "")
                        return;d
                    var file = this.files[0];
                    var reader = new FileReader();
                    this.value = "";
                    var thisInput = this;
                    
                    reader.onload = function(evt) 
                    {
	                    if (evt.target.readyState != 2)
		                    return;

	                    if (evt.target.error) 
	                    {
		                    alert("Error while reading file " + file.name + ": " + loadEvent.target.error);
		                    return;
	                    }

                        unityInstance.SendMessage(
                            inputLoader.gameObjName, 
                            inputLoader.dataSinkFn, 
                            evt.target.result);
                    }
                    reader.readAsText(file);
                }
        }
        inputLoader.gameObjName = Pointer_stringify(gameObjName);
        inputLoader.dataSinkFn = Pointer_stringify(dataSinkFn);
        inputLoader.setAttribute("accept", Pointer_stringify(extFilter))
        inputLoader.click();
    },
});
