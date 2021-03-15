export function viewToBuffer(view) {
    return view.buffer.slice(view.byteOffset, view.byteOffset + view.byteLength);
}
