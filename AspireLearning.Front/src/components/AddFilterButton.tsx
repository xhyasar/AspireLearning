import { useState, useRef } from "react";
import { useFilterContext, FilterOption } from "../context/FilterContext";
import { useClickOutside } from "../hooks/useClickOutside";

interface AddFilterButtonProps<T> {
  availableFilters: FilterOption<T>[];
}

// Basit bir filtre ikonu SVG'si
const FilterIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
    <path strokeLinecap="round" strokeLinejoin="round" d="M12 3c2.755 0 5.455.232 8.083.678.533.09.917.556.917 1.096v1.044a2.25 2.25 0 01-.659 1.591l-5.432 5.432a2.25 2.25 0 00-.659 1.591v2.927a2.25 2.25 0 01-1.244 2.013L9.75 21v-6.572a2.25 2.25 0 00-.659-1.591L3.659 7.409A2.25 2.25 0 013 5.818V4.774c0-.54.384-1.006.917-1.096A48.32 48.32 0 0112 3z" />
  </svg>
);

export default function AddFilterButton<T>({ availableFilters }: AddFilterButtonProps<T>) {
  const [isOpen, setIsOpen] = useState(false);
  const { activeFilters, addFilterKey } = useFilterContext<T>();
  const buttonRef = useRef<HTMLDivElement>(null);

  useClickOutside<HTMLDivElement>(buttonRef, () => setIsOpen(false));

  const availableOptions = availableFilters.filter(
    (filter) => !activeFilters.includes(filter.key)
  );

  if (availableOptions.length === 0) return null;

  return (
    <div className="relative" ref={buttonRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="bg-white border border-gray-300 text-[#1E90FF] px-3 rounded-[10px] hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-[#1E90FF] focus:ring-offset-2 flex items-center justify-center gap-2 text-sm w-[140px] h-[36px]"
      >
        <FilterIcon />
        Add Filter
      </button>

      {isOpen && (
        <div className="absolute z-10 mt-2 w-48 rounded-[10px] shadow-lg bg-white ring-1 ring-gray-666 ring-opacity-5">
          <div className="py-1">
            {availableOptions.map((filter) => (
              <button
                key={String(filter.key)}
                onClick={() => {
                  addFilterKey(filter.key);
                  setIsOpen(false);
                }}
                className="block w-full text-left px-4 py-2 text-sm text-gray-666 hover:bg-gray-100"
              >
                {filter.label}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}