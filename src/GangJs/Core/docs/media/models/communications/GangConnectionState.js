export var GangConnectionState;
(function (GangConnectionState) {
    GangConnectionState[GangConnectionState["connecting"] = 0] = "connecting";
    GangConnectionState[GangConnectionState["connected"] = 1] = "connected";
    GangConnectionState[GangConnectionState["disconnected"] = 2] = "disconnected";
    GangConnectionState[GangConnectionState["error"] = 3] = "error";
})(GangConnectionState || (GangConnectionState = {}));
