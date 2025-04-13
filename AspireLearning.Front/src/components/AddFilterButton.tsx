import { useFilterContext } from "../context/FilterContext";
import { useState } from "react";

const allFilters = ["status", "country", "city", "category"];

export default function AddFilterButton() {
  const { activeFilters, addFilterKey } = useFilterContext();
  const [open, setOpen] = useState(false);

  const availableFilters = allFilters.filter(f => !activeFilters.includes(f as any));

  return (
    <div className="relative">
      <button onClick={() => setOpen(!open)} className="border px-4 py-2 rounded">
        + Add Filter
      </button>
      {open && (
        <div className="absolute mt-2 bg-white border rounded shadow z-10 p-2">
          {availableFilters.map(f => (
            <div key={f}>
              <button
                onClick={() => {
                  addFilterKey(f as any);
                  setOpen(false);
                }}
                className="text-left w-full px-2 py-1 hover:bg-gray-100"
              >
                {f.charAt(0).toUpperCase() + f.slice(1)}
              </button>
            </div>
          ))}
          {availableFilters.length === 0 && (
            <div className="text-gray-400 px-2 py-1">All filters added</div>
          )}
        </div>
      )}
    </div>
  );
}