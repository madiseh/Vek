export async function downloadFileFromStream(fileName: string, contentType: string, streamReference: { arrayBuffer: () => Promise<ArrayBuffer> }): Promise<void> {
    const buffer = await streamReference.arrayBuffer();
    const blob = new Blob([buffer], { type: contentType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    document.body.append(anchor);
    anchor.click();
    anchor.remove();
    window.setTimeout(() => URL.revokeObjectURL(url), 0);
}
