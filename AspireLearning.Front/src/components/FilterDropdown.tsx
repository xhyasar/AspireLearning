import { useState } from "react";
import { useFilterContext } from "../context/FilterContext";

interface Props {
    label: string;
    options: string[];
}

export default function FilterDropdown({ label, options }: Props) {
    const key = label.toLowerCase() as any;
    const { filterValues, updateFilterValues, removeFilterKey } = useFilterContext();
    const [open, setOpen] = useState(false);

    const selected = filterValues[key] || [];

    const toggleValue = (value: string) => {
        if (selected.includes(value)) {
            updateFilterValues(key, selected.filter((v) => v !== value));
        } else {
            updateFilterValues(key, [...selected, value]);
        }
    };

    return (
        <div className="relative">
            <button
                onClick={() => setOpen(!open)}
                className="bg-white border px-3 py-2 rounded-full flex items-center gap-2"
            >
                {label}
                {selected.length > 0 && (
                    <span className="text-xs text-gray-500">({selected.length})</span>
                )}
                <span className="ml-1 text-gray-400">{open ? "▲" : "▼"}</span>
                <button
                    onClick={(e) => {
                        e.stopPropagation();
                        removeFilterKey(key);
                    }}
                    className="ml-2 text-red-500"
                >
                    ×
                </button>
            </button>

            {open && (
                <div className="absolute z-20 mt-2 bg-white border rounded shadow p-3 min-w-[200px]">
                    <div className="flex flex-col gap-2">
                        {options.map((opt) => (
                            <label key={opt} className="flex items-center gap-2">
                                <input
                                    type="checkbox"
                                    checked={selected.includes(opt)}
                                    onChange={() => toggleValue(opt)}
                                />
                                {opt}
                            </label>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}