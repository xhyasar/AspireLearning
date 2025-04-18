// src/components/DropdownFilterPill.tsx
import { useState } from "react";
import { useFilterContext } from "../context/FilterContext";

interface FilterItem {
    key: string;
    value: string;
}

interface Props {
    label: string;
    options: string[];
}

export default function DropdownFilterPill({ label, options }: Props) {
    const [open, setOpen] = useState(false);
    const key = label.toLowerCase() as FilterItem["key"];
    const { filterValues, updateFilterValues } = useFilterContext<Record<string, any>>();

    const selectedValues = filterValues[key] || [];

    const toggleValue = (value: string) => {
        const exists = selectedValues.includes(value);
        if (exists) {
            updateFilterValues(key, selectedValues.filter(v => v !== value));
        } else {
            updateFilterValues(key, [...selectedValues, value]);
        }
    };

    const handleClose = (value: string) => {
        updateFilterValues(key, selectedValues.filter(v => v !== value));
    };

    const hasAny = selectedValues.length > 0;

    if (!hasAny && !open) return null;

    return (
        <div className="relative">
            <button
                onClick={() => setOpen(!open)}
                className="bg-white border px-3 py-1 rounded-full flex items-center gap-2"
            >
                {label}
                {hasAny && <span className="text-xs text-gray-500">({selectedValues.length})</span>}
            </button>

            {open && (
                <div className="absolute z-20 mt-2 bg-white border rounded shadow p-3 min-w-[200px]">
                    <div className="mb-2 font-medium text-gray-600">{label}</div>
                    <div className="flex flex-col gap-2">
                        {options.map((opt) => (
                            <div key={opt} className="flex items-center justify-between">
                                <label className="flex items-center gap-2">
                                    <input
                                        type="checkbox"
                                        checked={selectedValues.includes(opt)}
                                        onChange={() => toggleValue(opt)}
                                    />
                                    <span>{opt}</span>
                                </label>
                                {selectedValues.includes(opt) && (
                                    <button
                                        className="text-red-500 text-sm"
                                        onClick={() => handleClose(opt)}
                                        title="Kaldır"
                                    >
                                        ×
                                    </button>
                                )}
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}
