// src/context/FilterContext.tsx
import { createContext, useContext, useState, useMemo } from "react";

export interface FilterOption<T> {
  key: keyof T;
  label: string;
  options: string[];
}

export interface FilterContextType<T> {
  activeFilters: (keyof T)[];
  filterValues: Record<keyof T, string[]>;
  addFilterKey: (key: keyof T) => void;
  removeFilterKey: (key: keyof T) => void;
  updateFilterValues: (key: keyof T, values: string[]) => void;
}

// Context'i null başlangıç değeriyle oluştur
const FilterContext = createContext<FilterContextType<any> | null>(null);

export function FilterProvider<T extends Record<string, any>>({ 
  children,
  filterOptions
}: { 
  children: React.ReactNode;
  filterOptions: FilterOption<T>[];
}) {
  const [activeFilters, setActiveFilters] = useState<(keyof T)[]>([]);
  const [filterValues, setFilterValues] = useState<Record<keyof T, string[]>>(() =>
    filterOptions.reduce((acc, option) => {
      acc[option.key] = [];
      return acc;
    }, {} as Record<keyof T, string[]>)
  );

  const addFilterKey = (key: keyof T) => {
    if (!activeFilters.includes(key)) {
      setActiveFilters((prev) => [...prev, key]);
    }
  };

  const removeFilterKey = (key: keyof T) => {
    setActiveFilters((prev) => prev.filter((k) => k !== key));
    setFilterValues((prev) => ({ ...prev, [key]: [] }));
  };

  const updateFilterValues = (key: keyof T, values: string[]) => {
    setFilterValues((prev) => ({ ...prev, [key]: values }));
  };

  // Context değerini useMemo ile optimize et
  const contextValue = useMemo(() => ({
    activeFilters,
    filterValues,
    addFilterKey,
    removeFilterKey,
    updateFilterValues
  }), [activeFilters, filterValues]);

  return (
    <FilterContext.Provider value={contextValue as FilterContextType<T>}>
      {children}
    </FilterContext.Provider>
  );
}

export function useFilterContext<T>() {
  const ctx = useContext(FilterContext);
  if (!ctx) {
    throw new Error("useFilterContext must be used within FilterProvider");
  }
  // Burada T tipine cast etmek yerine, Provider'da doğru tipi sağlıyoruz
  return ctx as FilterContextType<T>;
}
