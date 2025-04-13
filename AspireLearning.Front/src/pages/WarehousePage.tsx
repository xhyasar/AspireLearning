import { useState } from "react";
import { FilterProvider, useFilterContext } from "../context/FilterContext";
import AddFilterButton from "../components/AddFilterButton";
import FilterDropdown from "../components/FilterDropdown";
import WarehouseTable from "../components/WarehouseTable";

// Tüm filtre seçenekleri
const filterOptions: Record<string, string[]> = {
  status: ["active", "inactive", "warning"],
  country: ["USA", "Türkiye", "India"],
  city: ["Istanbul", "Ankara", "New York"],
  category: ["Main", "Transit", "Return"],
};

// 🎯 Filtre dropdown alanı
function FilterArea() {
  const { activeFilters } = useFilterContext();

  return (
      <div className="flex flex-wrap items-start gap-3 mb-6">
        {activeFilters.map((key) => (
            <FilterDropdown
                key={key}
                label={key.charAt(0).toUpperCase() + key.slice(1)}
                options={filterOptions[key]}
            />
        ))}
        <AddFilterButton />
      </div>
  );
}

// 🚀 Ana sayfa
export default function WarehousePage() {
  const [searchTerm, setSearchTerm] = useState("");

  return (
      <FilterProvider>
        <div className="p-6">
          {/* Üst Arama Kutusu */}
          <div className="mb-4">
            <input
                type="text"
                placeholder="Search by name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-72 px-4 py-2 border rounded"
            />
          </div>

          {/* 🔁 Filtreler ve Add Filter butonu aynı satırda */}
          <FilterArea />

          {/* Tablo */}
          <WarehouseTable search={searchTerm} />
        </div>
      </FilterProvider>
  );
}
