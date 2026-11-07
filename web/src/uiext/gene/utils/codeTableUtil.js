export function normalizeCodeOptions(list) {
    if (!Array.isArray(list)) {
        return [];
    }
    return list
        .map((item) => ({
            label: String(item?.label ?? item?.text ?? item?.name ?? item?.CodeName ?? item?.value ?? ""),
            value: String(item?.value ?? item?.key ?? item?.code ?? item?.Code ?? item?.id ?? item?.ID ?? "")
        }))
        .filter((item) => item.value !== "");
}

export function buildDictOptionsWithFallback(list, fallbackMap) {
    const base = normalizeCodeOptions(list);
    const map = new Map();
    base.forEach((item) => {
        const key = String(item?.value ?? "").trim();
        if (key) {
            map.set(key, { label: String(item.label ?? ""), value: key });
        }
    });

    if (fallbackMap && typeof fallbackMap === "object") {
        Object.keys(fallbackMap).forEach((key) => {
            const keyStr = String(key).trim();
            if (!keyStr) {
                return;
            }
            if (!map.has(keyStr)) {
                map.set(keyStr, { label: String(fallbackMap[key]), value: keyStr });
            }
        });
    }

    return Array.from(map.values());
}
