import { expect, test } from "bun:test";
import { readFileSync } from "node:fs";

const bundlePath = new URL("../wwwroot/Vek.min.js", import.meta.url);

test("generated JS bundle registers NumericField for Blazor interop", () => {
    const bundle = readFileSync(bundlePath, "utf8");

    expect(bundle).toContain("window.vekNumericField=");
});
