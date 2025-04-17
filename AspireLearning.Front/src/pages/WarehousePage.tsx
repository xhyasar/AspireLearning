import { useState } from "react";
import { FilterProvider, useFilterContext } from "../context/FilterContext";
import AddFilterButton from "../components/AddFilterButton";
import FilterDropdown from "../components/FilterDropdown";
import GenericTable from "../components/GenericTable";
import { warehouses } from "../mock/warehouses";
import activeIcon from "../assets/active.png";
import inactiveIcon from "../assets/inactive.png";
import warningIcon from "../assets/warning.png";

type Warehouse = typeof warehouses[0];

// Filtre seçenekleri
const filterOptions = [
  {
    key: 'status' as const,
    label: 'Status',
    options: ["active", "inactive", "warning"]
  },
  {
    key: 'country' as const,
    label: 'Country',
    options: ["USA", "Türkiye", "India", "Canada", "China", "Russia", "Belgium", "Argentina"]
  },
  {
    key: 'city' as const,
    label: 'City',
    options: ["Istanbul", "Ankara", "New York", "Philadelphia", "Jeromeworth", "Dunwoody", "Lake Eve", "Cleveton", "Kesslerburgh", "East Dewayne", "Bethesda"]
  },
  {
    key: 'category' as const,
    label: 'Category',
    options: ["Main", "Transit", "Return", "Distribution"]
  }
];

// İkonlar
const SearchIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="#1E90FF" /* color-blue-icon */ className="w-5 h-5">
    <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
  </svg>
);
const ExportIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
    <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5M16.5 12L12 16.5m0 0L7.5 12m4.5 4.5V3" />
  </svg>
);

// 🎯 Filtre dropdown alanı
function FilterArea() {
  const { activeFilters, removeFilterKey } = useFilterContext<Warehouse>();

  const handleClearFilters = () => {
    activeFilters.forEach(key => removeFilterKey(key));
  };

  return (
    <div className="flex flex-wrap items-center gap-3">
      {activeFilters.map((key) => {
        const filterOption = filterOptions.find(opt => opt.key === key);
        if (!filterOption) return null;
        
        return (
          <FilterDropdown<Warehouse>
            key={String(key)}
            label={filterOption.label}
            options={filterOption.options}
            filterKey={key}
          />
        );
      })}
      <AddFilterButton<Warehouse> availableFilters={filterOptions} />

      {activeFilters.length > 0 && (
        <button
          onClick={handleClearFilters}
          className="bg-white border border-gray-300 text-[#1E90FF] px-3 rounded-[10px] hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-[#1E90FF] focus:ring-offset-2 flex items-center justify-center gap-2 text-sm w-[95px] h-[36px]"
        >
          Clear Filter
        </button>
      )}
    </div>
  );
}

// 🚀 Ana sayfa içeriği
function WarehouseContent() {
  const [searchTerm, setSearchTerm] = useState("");
  const { filterValues } = useFilterContext<Warehouse>();
  const [rowsPerPage, setRowsPerPageState] = useState(10);
  const [currentPage, setCurrentPage] = useState(1);

  const columns = [
    { 
      key: 'name' as const, 
      header: 'Name', 
      sortable: true, 
      render: (item: Warehouse) => (
        <span className="text-[#1E90FF] cursor-pointer hover:underline">
          {item.name}
        </span>
      )
    },
    { key: 'country' as const, header: 'Country' },
    { key: 'city' as const, header: 'City' },
    { 
      key: 'status' as const, 
      header: 'Status',
      render: (item: Warehouse) => {
        const iconMap: Record<string, string> = {
          active: activeIcon,
          inactive: inactiveIcon,
          warning: warningIcon,
        };
        const iconSrc = iconMap[item.status];
        return iconSrc ? (
          <img
            src={iconSrc}
            alt={item.status}
            className="w-[24px] h-[24px] object-contain"
            title={item.status}
          />
        ) : null;
      }
    },
    { key: 'category' as const, header: 'Category' },
    { key: 'lastActivity' as const, header: 'Last Activity', sortable: true }
  ];

  const handleExport = () => {
    const headers = columns.map(col => col.header).join(',');
    const rows = warehouses.map(item => 
        columns.map(col => String(item[col.key])).join(',')
    ).join('\n');
    
    const csvContent = `${headers}\n${rows}`;
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'warehouses_export.csv';
    a.click();
    window.URL.revokeObjectURL(url);
    console.log("Export clicked");
  };

  const handleAdd = () => {
    console.log("Add warehouse clicked");
  };

  // rowsPerPage değiştiğinde currentPage'i 1'e sıfırla
  const handleRowsPerPageChange = (value: number) => {
    setRowsPerPageState(value);
    setCurrentPage(1); 
  };

  return (
    <div className="min-h-screen bg-white p-6">
      <div className="max-w-7xl mx-auto">
        {/* Sayfa Başlığı */}
        <h2 className="text-xl font-normal text-[#333333] mb-6 font-family-poppins tracking-wider "> 
          Warehouse Management
        </h2>

        {/* Üst Satır: Arama ve Sağ Kontroller (Add/Export) */}
        <div className="flex justify-between items-center mb-6"> 
          {/* Sol Taraf - Arama */}
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <SearchIcon />
            </div>
            <input
              type="text"
              placeholder="Search by name..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-72 pl-10 pr-4 py-2 border border-gray-300 rounded-[10px] shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent h-[36px]"
            />
          </div>
          
          {/* Sağ taraf - Add/Export Butonları */}
          <div className="flex items-center gap-4">
            <button
              onClick={handleAdd}
              className="bg-blue-500 text-white px-3 rounded-[10px] hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 flex items-center justify-center gap-2 text-sm w-[150px] h-[36px]"
            >
              <span className="text-lg">+</span>
              Add Warehouse
            </button>
            <button
              onClick={handleExport}
              className="bg-white border border-gray-300 text-gray-700 px-3 rounded-[10px] hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 flex items-center justify-center gap-2 text-sm w-[88px] h-[36px]"
            >
              <ExportIcon />
              Export
            </button>
          </div>
        </div>

        {/* Orta Satır: Filtreler ve Rows per Page */}
        <div className="flex justify-between items-center mb-6">
          {/* Sol Taraf - Filtreler */}
          <FilterArea />

          {/* Sağ Taraf - Rows per Page */}
          <div className="flex items-center gap-2">
            <span className="text-sm text-gray-600">Rows per page:</span>
            <select
              value={rowsPerPage}
              onChange={(e) => handleRowsPerPageChange(Number(e.target.value))}
              className="border border-gray-300 rounded-[10px] px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 h-[36px] text-[#1E90FF]"
              style={{}}
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
              <option value={100}>100</option>
            </select>
          </div>
        </div>

        {/* Tablo */}
        <div className="bg-white overflow-hidden">
          <GenericTable<Warehouse>
            data={warehouses}
            columns={columns}
            search={searchTerm}
            filterValues={filterValues}
            searchField="name"
            rowsPerPage={rowsPerPage}
            currentPage={currentPage}
            onPageChange={setCurrentPage}
          />
        </div>
      </div>
    </div>
  );
}

// 🚀 Ana sayfa
export default function WarehousePage() {
  return (
    <FilterProvider<Warehouse> filterOptions={filterOptions}>
      <WarehouseContent />
    </FilterProvider>
  );
}
