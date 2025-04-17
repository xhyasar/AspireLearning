import { useState, useRef } from "react";
import { useFilterContext } from "../context/FilterContext";
import { useClickOutside } from "../hooks/useClickOutside";

// İkonlar
const ChevronUpIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
    <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 15.75l7.5-7.5 7.5 7.5" />
  </svg>
);
const ChevronDownIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
    <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5" />
  </svg>
);
// Checkbox ikonları kaldırıldı

interface FilterDropdownProps<T> {
    label: string;
    options: string[];
    filterKey: keyof T;
}

export default function FilterDropdown<T>({ label, options, filterKey }: FilterDropdownProps<T>) {
    const { filterValues, updateFilterValues, removeFilterKey } = useFilterContext<T>();
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    useClickOutside<HTMLDivElement>(dropdownRef, () => setIsOpen(false));

    const selected = filterValues[filterKey] || [];

    const toggleValue = (value: string) => {
        if (selected.includes(value)) {
            updateFilterValues(filterKey, selected.filter((v) => v !== value));
        } else {
            updateFilterValues(filterKey, [...selected, value]);
        }
    };

    return (
        <div className="relative" ref={dropdownRef}>
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="bg-white border border-gray-300 px-3 rounded-[10px] flex items-center gap-2 hover:bg-gray-50 focus:outline-none h-[36px] text-sm text-gray-700"
            >
                {label}
                {selected.length > 0 && (
                    <span className="text-xs text-gray-500">({selected.length})</span>
                )}
                <span className="ml-1 text-gray-400">
                    {isOpen ? <ChevronUpIcon /> : <ChevronDownIcon />}
                </span>
                <button
                    onClick={(e) => {
                        e.stopPropagation();
                        removeFilterKey(filterKey);
                    }}
                    className="ml-auto text-red-500 hover:text-red-600 p-1 -mr-1"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-3.5 h-3.5">
                      <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>
            </button>

            {isOpen && (
                <div className="absolute z-20 mt-2 bg-white border border-[var(--color-primary)] rounded-[10px] shadow p-3 min-w-[200px]">
                    <div className="flex flex-col gap-2">
                        {options.map((opt) => (
                            <label key={opt} className="flex items-center gap-2 cursor-pointer hover:bg-gray-50 p-1 rounded">
                                {/* Varsayılan checkbox geri yüklendi */}
                                <input
                                    type="checkbox"
                                    checked={selected.includes(opt)}
                                    onChange={() => toggleValue(opt)}
                                    // Önceki stil: "rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                                    // İsterseniz buraya stil ekleyebilirsiniz, veya varsayılan bırakılabilir.
                                    // Örneğin, rengini primary yapmak için: className="rounded border-gray-300 text-[var(--color-primary)] focus:ring-[var(--color-primary)]"
                                    className="rounded border-gray-300 text-[var(--color-primary)] focus:ring-[var(--color-primary)]"
                                />
                                {/* Özel ikon alanı kaldırıldı */}
                                <span className="text-sm">{opt}</span>
                            </label>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}