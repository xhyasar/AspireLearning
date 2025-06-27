interface TableControlsProps {
    onExport: () => void;
    onAdd: () => void;
    rowsPerPage: number;
    onRowsPerPageChange: (value: number) => void;
}

export default function TableControls({ 
    onExport, 
    onAdd, 
    rowsPerPage, 
    onRowsPerPageChange 
}: TableControlsProps) {
    return (
        <div className="flex justify-between items-center mb-4">
            <div className="flex gap-2">
                <button
                    onClick={onAdd}
                    className="bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 flex items-center gap-2"
                >
                    <span className="text-lg">+</span>
                    Add Warehouse
                </button>
                <button
                    onClick={onExport}
                    className="bg-white border border-gray-333 text-gray-666 px-4 py-2 rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 flex items-center gap-2"
                >
                    <span>↓</span>
                    Export
                </button>
            </div>
            <div className="flex items-center gap-2">
                <span className="text-sm text-gray-666">Rows per page:</span>
                <select
                    value={rowsPerPage}
                    onChange={(e) => onRowsPerPageChange(Number(e.target.value))}
                    className="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                    <option value={10}>10</option>
                    <option value={20}>20</option>
                    <option value={50}>50</option>
                    <option value={100}>100</option>
                </select>
            </div>
        </div>
    );
} 