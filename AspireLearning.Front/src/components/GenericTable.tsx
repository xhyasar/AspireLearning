import {useState} from "react";
import arrowIcon from "../assets/arrow.png";
import Pagination from "./Pagination";

interface Column<T> {
    key: keyof T;
    header: string;
    sortable?: boolean;
    render?: (item: T) => React.ReactNode;
}

interface GenericTableProps<T> {
    data: T[];
    columns: Column<T>[];
    search: string;
    filterValues: Record<keyof T, string[]>;
    searchField: keyof T;
    onExport?: () => void;
    onAdd?: () => void;
    rowsPerPage?: number;
    currentPage: number;
    onPageChange: (page: number) => void;
}

export default function GenericTable<T>({ 
    data, 
    columns, 
    search, 
    filterValues,
    searchField,
    rowsPerPage = 10,
    currentPage,
    onPageChange
}: GenericTableProps<T>) {
    const [sortField, setSortField] = useState<keyof T | null>(null);
    const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");

    const filteredData = data.filter((item) => {
        const searchValue = String(item[searchField]).toLowerCase();
        const searchTerm = search.toLowerCase();

        const otherFiltersPass = (
            Object.entries(filterValues) as [string, string[]][]
        ).every(
            ([key, selectedValues]) =>
                selectedValues.length === 0 || selectedValues.includes(String(item[key as keyof T]))
        );

        return searchValue.includes(searchTerm) && otherFiltersPass;
    });

    const sortedData = [...filteredData].sort((a, b) => {
        if (!sortField) return 0;

        // Helper function to parse DD.MM.YYYY string to a Date object
        const parseDate = (dateStr: string): Date | null => {
            try {
                const parts = dateStr.split('.');
                if (parts.length === 3) {
                    const day = parseInt(parts[0], 10);
                    const month = parseInt(parts[1], 10);
                    const year = parseInt(parts[2], 10);
                    if (!isNaN(day) && !isNaN(month) && !isNaN(year)) {
                        const date = new Date(year, month - 1, day);
                        if (
                            date.getFullYear() === year &&
                            date.getMonth() === month - 1 &&
                            date.getDate() === day
                        ) {
                            return date;
                        }
                    }
                }
            } catch (e) {
                console.error("Date parse error:", e);
            }
            return null;
        };

        // Özel sıralama mantığı
        if (sortField === 'name') {
            const numA = parseInt(String(a[sortField]).replace(/\D/g, ''), 10) || 0;
            const numB = parseInt(String(b[sortField]).replace(/\D/g, ''), 10) || 0;
            if (sortDirection === "asc") {
                return numA - numB;
            } else {
                return numB - numA;
            }
        } else if (sortField === 'lastActivity') {
            const dateA = parseDate(String(a[sortField]));
            const dateB = parseDate(String(b[sortField]));

            // Geçersiz tarihleri HER ZAMAN sona at
            if (!dateA && !dateB) return 0; // İkisi de geçersiz, eşit
            if (!dateA) return 1;          // Sadece A geçersiz, A sona gitsin
            if (!dateB) return -1;         // Sadece B geçersiz, B sona gitsin

            // İki tarih de geçerli, zaman damgalarını karşılaştır
            const timeA = dateA.getTime();
            const timeB = dateB.getTime();

            if (sortDirection === "asc") { // En eski önce
                return timeA - timeB;
            } else { // En yeni önce
                return timeB - timeA;
            }
        } else {
            const valueA = String(a[sortField]).toLowerCase();
            const valueB = String(b[sortField]).toLowerCase();
            if (valueA < valueB) return sortDirection === "asc" ? -1 : 1;
            if (valueA > valueB) return sortDirection === "asc" ? 1 : -1;
            return 0;
        }
    });

    const handleSort = (field: keyof T) => {
        if (sortField === field) {
            setSortDirection((prev: "asc" | "desc") => (prev === "asc" ? "desc" : "asc"));
        } else {
            setSortField(field);
            // Alanlara göre varsayılan sıralama yönü
            if (field === 'name') {
                setSortDirection("desc"); // İstenildiği gibi 10 -> 1
            } else if (field === 'lastActivity') {
                setSortDirection("desc"); // Varsayılan: En yeni önce
            } else {
                setSortDirection("asc"); // Diğerleri için varsayılan: A -> Z
            }
        }
        onPageChange(1);
    };

    const totalPages = Math.ceil(sortedData.length / rowsPerPage);
    const startIndex = (currentPage - 1) * rowsPerPage;
    const endIndex = startIndex + rowsPerPage;
    const paginatedData = sortedData.slice(startIndex, endIndex);

    return (
        <div>
            <div className="overflow-x-auto">
                <table className="w-full text-sm">
                    <thead className="text-left">
                        <tr>
                            {columns.map((column) => (
                                <th
                                    key={String(column.key)}
                                    onClick={() => column.sortable && handleSort(column.key)}
                                    className={`py-3 px-4 border-b border-gray-999 text-[#333333] text-sm font-roboto font-normal ${column.sortable ? 'cursor-pointer' : ''}`}
                                >
                                    {column.header}
                                    {column.sortable && (
                                        <img
                                            src={arrowIcon}
                                            alt="sort"
                                            className={`w-[14px] h-[14px] ml-2 inline-block transition-transform duration-200 ${
                                                sortDirection === "desc" ? "rotate-180" : ""
                                            }`}
                                        />
                                    )}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {paginatedData.map((item, index) => (
                            <tr
                                key={index}
                                className="border-t hover:bg-gray-50"
                            >
                                {columns.map((column) => (
                                    <td key={String(column.key)} className="py-3 px-4 border-b tracking-wider text-sm font-poppins border-gray-999">
                                        {column.render ? column.render(item) : String(item[column.key])}
                                    </td>
                                ))}
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                rowsPerPage={rowsPerPage}
                totalItems={sortedData.length}
                onPageChange={onPageChange}
            />
        </div>
    );
}