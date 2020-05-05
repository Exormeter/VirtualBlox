var PDFRenderer = 
{
	PDFRenderer_loadedPages: [],
	PDFRenderer_loadedDocuments: [],
	PDFRenderer_loadedCanvas: [],
	PDFRenderer_workers: [],
     
	PDFJS_InitLibrary: function()
	{
		var script = document.createElement("script")
		script.type = "text/javascript";
		
		if(script.readyState) 
		{  
			//IE
			script.onreadystatechange = function() 
			{
				if (script.readyState === "loaded" || script.readyState === "complete") 
				{
					script.onreadystatechange = null;
					
					PDFJS.workerSrc = 'pdf.worker.js';
					
					SendMessage("WebGL_JSRuntime", 'OnLibraryInitialized', '');
				}
			};
		} 
		else 
		{  
			//Others
			script.onload = function()
			{
				PDFJS.workerSrc = 'pdf.worker.js';
				
				SendMessage("WebGL_JSRuntime", 'OnLibraryInitialized', '');
			};
		}
		
		script.src = 'pdf.js';
		document.getElementsByTagName("head")[0].appendChild(script);
		_PDFRenderer_Initialized = true;
	},
			
	PDFJS_LoadDocumentFromURL__deps: ['PDFRenderer_loadedDocuments'],
	PDFJS_LoadDocumentFromURL: function(promiseHandle, url)
	{
		var promiseHandleString = Pointer_stringify(promiseHandle);
		
		var loadingTask = PDFJS.getDocument(Pointer_stringify(url));
		
		loadingTask.onProgress = function(progress) {
		  SendMessage("WebGL_JSRuntime", "OnPromiseProgress", "promiseHandle: " + promiseHandleString + " progress: " + (progress.loaded / progress.total));
		}
		
		loadingTask.promise.then(function(pdf) 
		{
			var assignedIndex = -1;
			var length = _PDFRenderer_loadedDocuments.length;		
			for (var i = 0; i < length; i++) 
			{
				if (typeof _PDFRenderer_loadedDocuments[i] == 'undefined')
				{
					_PDFRenderer_loadedDocuments[i] = pdf;
					assignedIndex = i;
					break;
				}
			}
			if (assignedIndex == -1)
			{
				assignedIndex = _PDFRenderer_loadedDocuments.push(pdf) - 1;
			}
			
			SendMessage("WebGL_JSRuntime", "OnPromiseThen", "promiseHandle: " + promiseHandleString + " objectHandle: " + (assignedIndex + 1).toString());
		},function(reason)
		{
			SendMessage("WebGL_JSRuntime", "OnPromiseCatch", "promiseHandle: " + promiseHandleString + " objectHandle: " + "0");
		});
		
	},
	
	PDFJS_LoadDocumentFromBytes__deps: ['PDFRenderer_loadedDocuments'],
	PDFJS_LoadDocumentFromBytes: function(promiseHandle, base64)
	{
		var promiseHandleString = Pointer_stringify(promiseHandle);
		
		var raw = atob(Pointer_stringify(base64));
		var uint8Array = new Uint8Array(new ArrayBuffer(raw.length));

		for (var i = 0; i < raw.length; i++) 
		{
			uint8Array[i] = raw.charCodeAt(i);
		}
				
		PDFJS.getDocument(uint8Array).then(function(pdf) 
		{
			var assignedIndex = -1;
			var length = _PDFRenderer_loadedDocuments.length;		
			for (var i = 0; i < length; i++) 
			{
				if (typeof _PDFRenderer_loadedDocuments[i] == 'undefined')
				{
					_PDFRenderer_loadedDocuments[i] = pdf;
					assignedIndex = i;
					break;
				}
			}
			if (assignedIndex == -1)
			{
				assignedIndex = _PDFRenderer_loadedDocuments.push(pdf) - 1;
			}
			
			SendMessage("WebGL_JSRuntime", "OnPromiseThen", "promiseHandle: " + promiseHandleString + " objectHandle: " + (assignedIndex + 1).toString());
		},function(reason)
		{
			SendMessage("WebGL_JSRuntime", "OnPromiseCatch", "promiseHandle: " + promiseHandleString + " objectHandle: " + "0");
		});
	},
	
	PDFJS_CloseDocument__deps: ['PDFRenderer_loadedDocuments'],
	PDFJS_CloseDocument: function(documentHandle)
	{
		var pdfDocument = _PDFRenderer_loadedDocuments[documentHandle - 1];
		_PDFRenderer_loadedDocuments[documentHandle - 1] = undefined;
		delete pdfDocument;
	},
	
	PDFJS_GetPageCount__deps: ['PDFRenderer_loadedDocuments'],
	PDFJS_GetPageCount: function(documentHandle)
	{
		var pdfDocument = _PDFRenderer_loadedDocuments[documentHandle - 1];
		return pdfDocument.numPages;
	},
	
	PDFJS_LoadPage__deps: ['PDFRenderer_loadedDocuments', 'PDFRenderer_loadedPages'],
	PDFJS_LoadPage: function(promiseHandle, documentHandle, pageIndex)
	{
		var promiseHandleString = Pointer_stringify(promiseHandle);
		var pdfDocument = _PDFRenderer_loadedDocuments[documentHandle - 1];
		
		pdfDocument.getPage(pageIndex).then(function(page)
		{
			var assignedIndex = -1;
			var length = _PDFRenderer_loadedPages.length;		
			for (var i = 0; i < length; i++) 
			{
				if (typeof _PDFRenderer_loadedPages[i] == 'undefined')
				{
					_PDFRenderer_loadedPages[i] = page;
					assignedIndex = i;
					break;
				}
			}
			if (assignedIndex == -1)
			{
				assignedIndex = _PDFRenderer_loadedPages.push(page) - 1;
			}
			
			SendMessage("WebGL_JSRuntime", "OnPromiseThen", "promiseHandle: " + promiseHandleString + " objectHandle: " + (assignedIndex + 1).toString());
		},function(reason)
		{
			SendMessage("WebGL_JSRuntime", "OnPromiseCatch", "promiseHandle: " + promiseHandleString + " objectHandle: " + "0");
		});
	},
	
	PDFJS_ClosePage__deps: ['PDFRenderer_loadedPages'],
	PDFJS_ClosePage: function(pageHandle)
	{
		var page = _PDFRenderer_loadedPages[pageHandle - 1];
		_PDFRenderer_loadedPages[pageHandle - 1] = undefined;
		delete page;
	},
	
	PDFJS_TryTerminateRenderWorker__deps: ['PDFRenderer_workers'],
	PDFJS_TryTerminateRenderWorker: function(promiseHandle)
	{
		var worker = _PDFRenderer_workers[promiseHandle];
		if (worker != null && worker != undefined)
		{
			worker.cancel();
			
			delete _PDFRenderer_workers[promiseHandle];
			
			SendMessage("WebGL_JSRuntime", "OnPromiseCancel", "promiseHandle: " + promiseHandleString + " objectHandle: " + "0");
		}
	},
	
	PDFJS_RenderPageIntoCanvas__deps: ['PDFRenderer_loadedPages', 'PDFRenderer_loadedCanvas', 'PDFRenderer_workers'],
    PDFJS_RenderPageIntoCanvas: function(promiseHandle, pageHandle, scale, width, height)
    {
		var promiseHandleString = Pointer_stringify(promiseHandle);
		var page = _PDFRenderer_loadedPages[pageHandle - 1];
		var viewport = page.getViewport(scale);
		var canvas = document.createElement('canvas');
		var context = canvas.getContext('2d');
		
		context.canvas.width = width;
		context.canvas.height = height;

		var renderContext = 
		{
			canvasContext: context,
			viewport: viewport
		};
				
		_PDFRenderer_workers[promiseHandle] = page.render(renderContext).then(function()
		{
			var assignedIndex = -1;
			var length = _PDFRenderer_loadedCanvas.length;		
			for (var i = 0; i < length; i++) 
			{
				if (typeof _PDFRenderer_loadedCanvas[i] == 'undefined')
				{
					_PDFRenderer_loadedCanvas[i] = canvas;
					assignedIndex = i;
					break;
				}
			}
			if (assignedIndex == -1)
			{
				assignedIndex = _PDFRenderer_loadedCanvas.push(canvas) - 1;
			}
			
			delete _PDFRenderer_workers[promiseHandle];
						
			SendMessage("WebGL_JSRuntime", "OnPromiseThen", "promiseHandle: " + promiseHandleString + " objectHandle: " + (assignedIndex + 1).toString());
			
			
		},function(reason)
		{
			delete _PDFRenderer_workers[promiseHandle];
						
			SendMessage("WebGL_JSRuntime", "OnPromiseCatch", "promiseHandle: " + promiseHandleString + " objectHandle: " + "0");
		}).internalRenderTask;
    },
		
	PDFJS_RenderCanvasIntoTexture__deps: ['PDFRenderer_loadedCanvas'],
    PDFJS_RenderCanvasIntoTexture: function (canvasHandle, textureHandle)
	{
	    var canvas = _PDFRenderer_loadedCanvas[canvasHandle - 1];
		
		GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);	
        GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[textureHandle]);

		if (typeof WebGL2RenderingContext !== 'undefined' && GLctx instanceof WebGL2RenderingContext)
        {
            GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, canvas.width, canvas.height, GLctx.RGBA, GLctx.UNSIGNED_BYTE, canvas);
		}
		else
        {
			GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, canvas);
        }

		GLctx.bindTexture(GLctx.TEXTURE_2D, null);
        GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
	},
	
	PDFJS_DestroyCanvas__deps: ['PDFRenderer_loadedCanvas'],
	PDFJS_DestroyCanvas: function(canvasHandle)
	{
		var canvas = _PDFRenderer_loadedCanvas[canvasHandle - 1];
		delete canvas;
		_PDFRenderer_loadedCanvas[canvasHandle - 1] = undefined;
	},
	
	PDFJS_GetPageWidth__deps: ['PDFRenderer_loadedPages'],
	PDFJS_GetPageWidth: function(pageHandle, scale)
	{
		return _PDFRenderer_loadedPages[pageHandle - 1].getViewport(scale).width;
	},
	
	PDFJS_GetPageHeight__deps: ['PDFRenderer_loadedPages'],
	PDFJS_GetPageHeight: function(pageHandle, scale)
	{
		return _PDFRenderer_loadedPages[pageHandle - 1].getViewport(scale).height;
	}
};

mergeInto(LibraryManager.library, PDFRenderer);