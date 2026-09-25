/****************************************************************************
 * CRI Middleware SDK
 * 
 * Copyright (c) 2016 CRI Middleware Co., Ltd.
 * 
 * Library  : CRI File System
 * Module   : I/O interface of JavaScript library
 * File     : crifs_io.jslib
 ****************************************************************************/

var LibraryCriFsIo = {
    /*==========================================================================
     * Main File I/O Manager
     * Handles file operations for both browser and WeChat environments
     *========================================================================*/
    $CriFsIo: {
        // File tracking
        fileList: {},
        
        // WeChat file system manager
        fsManager: null,
        
        // File status constants
        STATUS: {
            INITIALIZED: "initialized",
            LOADING: "loading",
            COMPLETED: "completed"
        },
        
        /*----------------------------------------------------------------------
         * Browser-specific file fetching
         *--------------------------------------------------------------------*/
        /**
         * Fetch file from URL (browser only)
         * @param {string} filePath - URL of the file
         * @param {boolean} head - Whether to perform HEAD request only
         * @param {{offset: number, size: number}|null} range - Byte range for partial content
         * @returns {Promise<{fileSize: number, eTag: string|null, data: ArrayBuffer|null}>}
         */
        fetchFile: async function(filePath, head = false, range = null) {
            // Skip if in WeChat environment
            if (typeof wx !== 'undefined') return null;
            
            try {
                const options = head ? { method: 'HEAD' } : {};
                
                // Add range header if specified
                if (range && typeof range.offset === 'number' && typeof range.size === 'number') {
                    const start = range.offset;
                    const end = range.offset + range.size - 1;
                    if (!options.headers) {
                        options.headers = {};
                    }
                    options.headers['Range'] = `bytes=${start}-${end}`;
                }
                
                const response = await fetch(filePath, options);
                
                // Check response status
                if (!response.ok && response.status !== 206 && !head) {
                    throw new Error(`Failed to fetch ${filePath}. Status: ${response.status}`);
                }
                
                const fileSize = parseInt(response.headers.get("Content-Length") || "-1");
                const eTag = response.headers.get("ETag");
                const data = head ? null : await response.arrayBuffer();

                // If we requested a range but server returned full content (200 instead of 206),
                // extract the requested range from the full data
                if (data && range && response.status === 200) {
                    const start = range.offset;
                    const end = Math.min(range.offset + range.size, data.byteLength);
                    return { fileSize: data.byteLength, eTag, data: data.slice(start, end) };
                }

                return { fileSize, eTag, data };
            } catch (error) {
                console.error(`Error fetching ${filePath}:`, error);
                return { fileSize: -1, eTag: null, data: null };
            }
        },
        
        /*----------------------------------------------------------------------
         * WeChat-specific helpers
         *--------------------------------------------------------------------*/
        /**
         * Convert URL to relative path for WeChat file system
         * @param {string} url - Full URL
         * @param {string} subpathToRemove - Path prefix to remove
         * @returns {string|null} Relative path or null
         */
        getRelativePath: function(url, subpathToRemove) {
            // Skip if not in WeChat environment
            if (typeof wx === 'undefined') return null;

            try {
                // Extract pathname without URL constructor (not available in WeChat)
                var relativePath;
                var protoIdx = url.indexOf('://');
                if (protoIdx !== -1) {
                    var pathStart = url.indexOf('/', protoIdx + 3);
                    relativePath = pathStart !== -1 ? url.substring(pathStart + 1) : '';
                } else {
                    relativePath = url.replace(/^\/+/, '');
                }

                // Remove specified subpath if present
                if (relativePath.startsWith(subpathToRemove)) {
                    relativePath = relativePath.substring(subpathToRemove.length);
                }

                return relativePath.replace(/^\/+/, '');
            } catch (e) {
                console.error("Invalid URL:", e);
                return null;
            }
        },
        
        /**
         * Initialize file info in fileList
         * @param {string} filePath - Path of the file
         * @returns {Object} File info object
         */
        initFileInfo: function(filePath) {
            if (!(filePath in CriFsIo.fileList)) {
                CriFsIo.fileList[filePath] = {
                    fileSize: -1,
                    data: null,
                    status: CriFsIo.STATUS.INITIALIZED,
                    handleList: {},
                    eTag: null
                };
            }
            return CriFsIo.fileList[filePath];
        },
        
        /*----------------------------------------------------------------------
         * WeChat file loading implementation
         *--------------------------------------------------------------------*/
        /**
         * Load file in WeChat environment
         * @param {string} filePath - Path of the file
         * @param {Object} fileInfo - File info object
         */
        loadWeChat: function(filePath, fileInfo) {
            // Initialize file system manager if needed
            if (CriFsIo.fsManager === null) {
                CriFsIo.fsManager = wx['getFileSystemManager']();
            }
            
            // Skip if already loading
            if (fileInfo.status === CriFsIo.STATUS.LOADING) {
                return;
            }
            
            // Skip if already loaded
            if (fileInfo.fileSize !== -1) {
                return;
            }
            
            fileInfo.status = CriFsIo.STATUS.LOADING;
            
            const isLocalhost = filePath.includes('localhost') || filePath.includes('127.0.0.1');
            
            if (isLocalhost) {
                // Handle local file
                CriFsIo.loadLocalFileWeChat(filePath, fileInfo);
            } else {
                // Handle remote file
                CriFsIo.loadRemoteFileWeChat(filePath, fileInfo);
            }
        },
        
        /**
         * Load local file in WeChat
         * @param {string} filePath - Path of the file
         * @param {Object} fileInfo - File info object
         */
        loadLocalFileWeChat: function(filePath, fileInfo) {
            const absoluteFilePath = CriFsIo.getRelativePath(filePath, "game/");

            CriFsIo.fsManager['readFile']({
                'filePath': absoluteFilePath,
                'success': (result) => {
                    fileInfo.fileSize = result['data']['byteLength'];
                    fileInfo.data = result['data'];
                },
                'fail': (result) => {
                    console.error(`Error loading local file ${filePath}:`, result);
                    fileInfo.fileSize = -1;
                    fileInfo.data = null;
                },
                'complete': () => {
                    fileInfo.status = CriFsIo.STATUS.COMPLETED;
                }
            });
        },
        
        /**
         * Load remote file in WeChat
         * @param {string} filePath - Path of the file
         * @param {Object} fileInfo - File info object
         */
        loadRemoteFileWeChat: function(filePath, fileInfo) {
            wx['downloadFile']({
                'url': filePath,
                'success': (res) => {
                    if (res['statusCode'] === 200) {
                        // Read the downloaded file
                        CriFsIo.fsManager['readFile']({
                            'filePath': res['tempFilePath'],
                            'success': (result) => {
                                fileInfo.fileSize = result['data']['byteLength'];
                                fileInfo.data = result['data'];
                            },
                            'fail': (result) => {
                                console.error(`Error reading downloaded file ${filePath}:`, result);
                                fileInfo.fileSize = -1;
                                fileInfo.data = null;
                            },
                            'complete': () => {
                                fileInfo.status = CriFsIo.STATUS.COMPLETED;
                            }
                        });
                    } else {
                        console.error(`Download failed for ${filePath}. Status code: ${res['statusCode']}`);
                        fileInfo.fileSize = -1;
                        fileInfo.data = null;
                        fileInfo.status = CriFsIo.STATUS.COMPLETED;
                    }
                },
                'fail': (error) => {
                    console.error(`Error downloading file ${filePath}:`, error);
                    fileInfo.fileSize = -1;
                    fileInfo.data = null;
                    fileInfo.status = CriFsIo.STATUS.COMPLETED;
                }
            });
        },
        
        /*----------------------------------------------------------------------
         * Browser file loading implementation
         *--------------------------------------------------------------------*/
        /**
         * Load file in browser environment
         * @param {string} filePath - Path of the file
         * @param {Object} fileInfo - File info object
         */
        loadBrowser: async function(filePath, fileInfo) {
            // Skip if already loading
            if (fileInfo.status === CriFsIo.STATUS.LOADING) {
                return;
            }
            
            // Skip if already loaded
            if (fileInfo.fileSize !== -1) {
                return;
            }
            
            try {
                fileInfo.status = CriFsIo.STATUS.LOADING;
                
                // Fetch file metadata (HEAD request)
                const { fileSize, eTag } = await CriFsIo.fetchFile(filePath, true);
                
                fileInfo.fileSize = fileSize;
                fileInfo.eTag = eTag;
                
            } catch (error) {
                console.error(`Error loading ${filePath}:`, error);
                fileInfo.fileSize = -1;
            } finally {
                fileInfo.status = CriFsIo.STATUS.COMPLETED;
            }
        },
        
        /*----------------------------------------------------------------------
         * Public API Methods
         *--------------------------------------------------------------------*/
        /**
         * Load a file (environment-aware)
         * @param {string} filePath - Path of the file to load
         */
        load: function(filePath) {
            const fileInfo = CriFsIo.initFileInfo(filePath);
            
            if (typeof wx !== 'undefined') {
                // WeChat environment
                CriFsIo.loadWeChat(filePath, fileInfo);
            } else {
                // Browser environment
                CriFsIo.loadBrowser(filePath, fileInfo);
            }
        },
        
        /**
         * Check if file is fully loaded
         * @param {string} filePath - Path of the file
         * @returns {boolean} true if loaded
         */
        isLoaded: function(filePath) {
            if (!(filePath in CriFsIo.fileList)) {
                return false;
            }
            const fileInfo = CriFsIo.fileList[filePath];
            return fileInfo.status === CriFsIo.STATUS.COMPLETED;
        },
        
        /**
         * Get file size
         * @param {string} filePath - Path of the file
         * @returns {number} File size in bytes or -1 if not available
         */
        getFileSize: function(filePath) {
            if (!(filePath in CriFsIo.fileList)) {
                return -1;
            }
            const fileInfo = CriFsIo.fileList[filePath];
            return fileInfo.fileSize;
        },
        
        /**
         * Read data from file
         * @param {number} handle - Request handle
         * @param {string} filePath - Path of the file
         * @param {number} offset - Byte offset to start reading
         * @param {number} length - Number of bytes to read
         * @param {number} pointer - Memory pointer to write data
         */
        read: function(handle, filePath, offset, length, pointer) {
            if (!(filePath in CriFsIo.fileList)) {
                throw new Error(`File ${filePath} not found in fileList`);
            }
            
            const fileInfo = CriFsIo.fileList[filePath];
            
            // Initialize or get request handle
            let requestHandle = fileInfo.handleList[handle];
            if (!requestHandle) {
                requestHandle = {
                    readSize: 0,
                    isCompleted: false
                };
                fileInfo.handleList[handle] = requestHandle;
            }
            
            // Reset handle state
            requestHandle.readSize = 0;
            requestHandle.isCompleted = false;
            
            if (typeof wx !== 'undefined') {
                // WeChat: Read from memory
                CriFsIo.readFromMemory(fileInfo.data, offset, length, pointer, requestHandle);
            } else {
                // Browser: Read asynchronously
                CriFsIo.readAsync(filePath, fileInfo, offset, length, pointer, requestHandle);
            }
        },
        
        /**
         * Read data from memory buffer
         * @param {ArrayBuffer} data - Source data
         * @param {number} offset - Byte offset
         * @param {number} length - Bytes to read
         * @param {number} pointer - Destination pointer
         * @param {Object} requestHandle - Request handle object
         */
        readFromMemory: function(data, offset, length, pointer, requestHandle) {
            if (!data) {
                requestHandle.readSize = -1;
                requestHandle.isCompleted = true;
                return;
            }
            
            requestHandle.readSize = Math.min(length, data.byteLength - offset);
            const responseArray = new Uint8Array(data.slice(offset, offset + requestHandle.readSize));
            const buffer = new Uint8Array(Module['HEAPU8'].buffer, pointer, requestHandle.readSize);
            buffer.set(responseArray);
            requestHandle.isCompleted = true;
        },
        
        /**
         * Read data asynchronously in browser
         * @param {string} filePath - Path of the file
         * @param {Object} fileInfo - File info object
         * @param {number} offset - Byte offset
         * @param {number} length - Bytes to read
         * @param {number} pointer - Destination pointer
         * @param {Object} requestHandle - Request handle object
         */
        readAsync: async function(filePath, fileInfo, offset, length, pointer, requestHandle) {
            try {
                // Fetch data with range request
                const response = await CriFsIo.fetchFile(filePath, false, { offset, size: length });
                
                if (response.data) {
                    requestHandle.readSize = Math.min(length, response.data.byteLength);
                    const responseArray = new Uint8Array(response.data, 0, requestHandle.readSize);
                    const buffer = new Uint8Array(Module['HEAPU8'].buffer, pointer, requestHandle.readSize);
                    buffer.set(responseArray);
                } else {
                    requestHandle.readSize = -1;
                }
                
            } catch (error) {
                console.error(`Error reading ${filePath}:`, error);
                requestHandle.readSize = -1;
            } finally {
                requestHandle.isCompleted = true;
            }
        },
        
        /**
         * Check if read operation is completed
         * @param {number} handle - Request handle
         * @param {string} filePath - Path of the file
         * @returns {boolean} true if completed
         */
        isReadCompleted: function(handle, filePath) {
            if (!(filePath in CriFsIo.fileList)) {
                return false;
            }
            
            const fileInfo = CriFsIo.fileList[filePath];
            const requestHandle = fileInfo.handleList[handle];
            
            return requestHandle ? requestHandle.isCompleted : false;
        },
        
        /**
         * Get number of bytes read
         * @param {number} handle - Request handle
         * @param {string} filePath - Path of the file
         * @returns {number} Bytes read or -1 on error
         */
        getReadSize: function(handle, filePath) {
            if (!(filePath in CriFsIo.fileList)) {
                return -1;
            }
            
            const fileInfo = CriFsIo.fileList[filePath];
            const requestHandle = fileInfo.handleList[handle];
            
            return requestHandle ? requestHandle.readSize : -1;
        },
        
        /**
         * Clean up resources for a handle
         * @param {number} handle - Request handle
         * @param {string} filePath - Path of the file
         */
        unload: function(handle, filePath) {
            if (!(filePath in CriFsIo.fileList)) {
                return;
            }
            
            const fileInfo = CriFsIo.fileList[filePath];
            delete fileInfo.handleList[handle];
        }
    },
    /*==========================================================================
     * External C/C++ Interface Functions
     * These are called from C/C++ code via Emscripten
     *========================================================================*/
    criFsIoJs_Load: function(path) {
        CriFsIo.load(UTF8ToString(path));
    },
    
    criFsIoJs_IsLoaded: function(path) {
        return CriFsIo.isLoaded(UTF8ToString(path));
    },
    
    criFsIoJs_GetFileSize: function(path) {
        return CriFsIo.getFileSize(UTF8ToString(path));
    },
    
    criFsIoJs_Unload: function(handle, path) {
        CriFsIo.unload(handle, UTF8ToString(path));
    },
    
    criFsIoJs_Read: function(handle, path, offset, length, pointer) {
        CriFsIo.read(handle, UTF8ToString(path), offset, length, pointer);
    },
    
    criFsIoJs_IsReadCompleted: function(handle, path) {
        return CriFsIo.isReadCompleted(handle, UTF8ToString(path));
    },
    
    criFsIoJs_GetReadSize: function(handle, path) {
        return CriFsIo.getReadSize(handle, UTF8ToString(path));
    }
};

// Register dependencies for Emscripten
autoAddDeps(LibraryCriFsIo, '$CriFsIo');
mergeInto(LibraryManager.library, LibraryCriFsIo);

/* --- end of file --- */