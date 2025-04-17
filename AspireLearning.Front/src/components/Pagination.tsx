import React from 'react';

// İkonlar
const FirstPageIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
    <path strokeLinecap="round" strokeLinejoin="round" d="M18.75 19.5l-7.5-7.5 7.5-7.5m-6 15L5.25 12l7.5-7.5" />
  </svg>
);
const PreviousPageIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
    <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
  </svg>
);
const NextPageIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
    <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
  </svg>
);
const LastPageIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-4 h-4">
    <path strokeLinecap="round" strokeLinejoin="round" d="M5.25 4.5l7.5 7.5-7.5 7.5m6-15l7.5 7.5-7.5 7.5" />
  </svg>
);

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    rowsPerPage: number;
    totalItems: number;
    onPageChange: (page: number) => void;
}

export default function Pagination({ 
    currentPage, 
    totalPages, 
    rowsPerPage, 
    totalItems,
    onPageChange 
}: PaginationProps) {
    
    const startRecord = (currentPage - 1) * rowsPerPage + 1;
    const endRecord = Math.min(currentPage * rowsPerPage, totalItems);

    return (
        <div className="flex items-center justify-end py-3 px-4 text-sm text-gray-666 gap-4">
            <div className="flex items-center gap-1">
                <button
                    onClick={() => onPageChange(1)}
                    disabled={currentPage === 1}
                    className={`p-1 rounded ${currentPage === 1 ? 'text-gray-666 cursor-not-allowed' : 'hover:bg-gray-100'}`}
                    aria-label="First Page"
                >
                    <FirstPageIcon />
                </button>
                <button
                    onClick={() => onPageChange(currentPage - 1)}
                    disabled={currentPage === 1}
                    className={`p-1 rounded ${currentPage === 1 ? 'text-gray-666 cursor-not-allowed' : 'hover:bg-gray-100'}`}
                    aria-label="Previous Page"
                >
                    <PreviousPageIcon />
                </button>
            </div>

            <span>
                Records from {startRecord} to {endRecord}
            </span>

            <div className="flex items-center gap-1">
                <button
                    onClick={() => onPageChange(currentPage + 1)}
                    disabled={currentPage === totalPages}
                    className={`p-1 rounded ${currentPage === totalPages ? 'text-gray-666 cursor-not-allowed' : 'hover:bg-gray-100'}`}
                    aria-label="Next Page"
                >
                    <NextPageIcon />
                </button>
                <button
                    onClick={() => onPageChange(totalPages)}
                    disabled={currentPage === totalPages}
                    className={`p-1 rounded ${currentPage === totalPages ? 'text-gray-666 cursor-not-allowed' : 'hover:bg-gray-100'}`}
                    aria-label="Last Page"
                >
                    <LastPageIcon />
                </button>
            </div>
        </div>
    );
} 