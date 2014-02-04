var canvas = null;
var quality = 0.1;

// Al recibir un mensaje se ejecuta
self.addEventListener('message', function (e) {
    self.canvas = e[0];
    if (self.canvas != null) {
        var r = self.canvas.toDataURL("image/jpeg", quality);
        self.postMessage(r);
    }
}, false);

