// src/context/FilterContext.tsx
import { createContext, useContext, useState } from "react";

export type FilterKey = "status" | "country" | "city" | "category";

interface FilterContextType {
  activeFilters: FilterKey[];
  filterValues: Record<FilterKey, string[]>;
  addFilterKey: (key: FilterKey) => void;
  removeFilterKey: (key: FilterKey) => void;
  updateFilterValues: (key: FilterKey, values: string[]) => void;
}

const FilterContext = createContext<FilterContextType | undefined>(undefined);

export const FilterProvider = ({ children }: { children: React.ReactNode }) => {
  const [activeFilters, setActiveFilters] = useState<FilterKey[]>([]);
  const [filterValues, setFilterValues] = useState<Record<FilterKey, string[]>>({
    status: [],
    country: [],
    city: [],
    category: [],
  });

  const addFilterKey = (key: FilterKey) => {
    if (!activeFilters.includes(key)) {
      setActiveFilters([...activeFilters, key]);
    }
  };

  const removeFilterKey = (key: FilterKey) => {
    setActiveFilters(activeFilters.filter((k) => k !== key));
    setFilterValues((prev) => ({ ...prev, [key]: [] }));
  };

  const updateFilterValues = (key: FilterKey, values: string[]) => {
    setFilterValues((prev) => ({ ...prev, [key]: values }));
  };

  return (
      <FilterContext.Provider
          value={{ activeFilters, filterValues, addFilterKey, removeFilterKey, updateFilterValues }}
      >
        {children}
      </FilterContext.Provider>
  );
};

export const useFilterContext = () => {
  const ctx = useContext(FilterContext);
  if (!ctx) throw new Error("useFilterContext must be used within FilterProvider");
  return ctx;
};
