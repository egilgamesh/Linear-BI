function addChart(chartType) {
    document.getElementById("chartOptions").style.display = "block";
    document.getElementById("tableOptions").style.display = "none";
    document.getElementById("chartType").value = chartType;
}

function addTable() {
    document.getElementById("chartOptions").style.display = "none";
    document.getElementById("tableOptions").style.display = "block";
}

// Modify the fetchChartData function to process the data
async function fetchChartData(apiURL) {
    try {
        const response = await fetch(apiURL);
        if (response.ok) {
            const data = await response.json();

            // Extract post titles and their character counts
            const chartData = data.map(post => ({
                label: post.title,
                value: post.title.length,
            }));

            return chartData;
        } else {
            throw new Error('Failed to fetch data from the API');
        }
    } catch (error) {
        console.error(error);
        return null;
    }
}

async function customizeChart() {
    const chartType = document.getElementById("chartType").value;
    const apiURL = document.getElementById("apiURL").value;
    const editor = document.getElementById("editor");
    const chartContainer = document.createElement("div");
    chartContainer.style.width = "600px";
    chartContainer.style.height = "400px";
    chartContainer.style.position = "relative"; // Add this line to set the position property to 'relative'
    chartContainer.style.border = "2px dashed #000"; // Add this line to create a border for the chart

    chartContainer.classList.add("chart-card");
    // Make the chart container draggable
    chartContainer.onmousedown = (event) => {
        isDragging = true;
        offsetX = event.clientX - chartContainer.getBoundingClientRect().left;
        offsetY = event.clientY - chartContainer.getBoundingClientRect().top;
    };

    chartContainer.onmouseup = () => {
        isDragging = false;
    };

    editor.appendChild(chartContainer);
    makeChartDraggable(chartContainer);

    const chartData = await fetchChartData(apiURL, chartType);

    if (chartData) {
        const width = 600;
        const height = 400;
        const margin = { top: 20, right: 20, bottom: 40, left: 40 };

        const svg = d3.select(chartContainer)
            .append("svg")
            .attr("width", width)
            .attr("height", height);

        const chartWidth = width - margin.left - margin.right;
        const chartHeight = height - margin.top - margin.bottom;

        const g = svg.append("g")
            .attr("transform", `translate(${margin.left}, ${margin.top})`);

        const xScale = d3.scaleBand()
            .domain(chartData.map(d => d.label))
            .range([0, chartWidth])
            .padding(0.1);

        const yScale = d3.scaleLinear()
            .domain([0, d3.max(chartData, d => d.value)])
            .nice()
            .range([chartHeight, 0]);

        if (chartType === 'bar') {
            g.selectAll(".bar")
                .data(chartData)
                .enter()
                .append("rect")
                .attr("class", "bar")
                .attr("x", d => xScale(d.label))
                .attr("y", d => yScale(d.value))
                .attr("width", xScale.bandwidth())
                .attr("height", d => chartHeight - yScale(d.value))
                .style("fill", "steelblue")
                .on("mouseover", function () {
                    d3.select(this).style("fill", "orange"); // Change the color on mouseover
                })
                .on("mouseout", function () {
                    d3.select(this).style("fill", "steelblue"); // Change it back on mouseout
                });

            g.append("g")
                .attr("class", "x-axis")
                .attr("transform", `translate(0, ${chartHeight})`)
                .call(d3.axisBottom(xScale));

            g.append("g")
                .attr("class", "y-axis")
                .call(d3.axisLeft(yScale));

            // Add labels and titles to your chart
            g.selectAll(".bar-label")
                .data(chartData)
                .enter()
                .append("text")
                .attr("class", "bar-label")
                .attr("x", d => xScale(d.label) + xScale.bandwidth() / 2)
                .attr("y", d => yScale(d.value) - 10) // Adjust the position
                .attr("text-anchor", "middle")
                .text(d => d.value)
                .style("fill", "black")
                .style("font-size", "12px");

            // Add title
            svg.append("text")
                .attr("x", width / 2)
                .attr("y", 10)
                .attr("text-anchor", "middle")
                .style("font-size", "16px")
                .text("Bar Chart Title");

        }
        else if (chartType === 'line') {
            const line = d3.line()
                .x(d => xScale(d.label))
                .y(d => yScale(d.value));

            g.append("path")
                .datum(chartData)
                .attr("class", "line")
                .attr("d", line)
                .attr("fill", "none")
                .attr("stroke", "steelblue");

            g.selectAll(".dot")
                .data(chartData)
                .enter()
                .append("circle")
                .attr("class", "dot")
                .attr("cx", d => xScale(d.label))
                .attr("cy", d => yScale(d.value))
                .attr("r", 5)
                .style("fill", "steelblue")
                .on("mouseover", function () {
                    d3.select(this).style("fill", "orange"); // Change the color on mouseover
                })
                .on("mouseout", function () {
                    d3.select(this).style("fill", "steelblue"); // Change it back on mouseout
                });

            g.append("g")
                .attr("class", "x-axis")
                .attr("transform", `translate(0, ${chartHeight})`)
                .call(d3.axisBottom(xScale));

            g.append("g")
                .attr("class", "y-axis")
                .call(d3.axisLeft(yScale));

            // Add labels and titles to your chart
            g.selectAll(".line-label")
                .data(chartData)
                .enter()
                .append("text")
                .attr("class", "line-label")
                .attr("x", d => xScale(d.label))
                .attr("y", d => yScale(d.value) - 15) // Adjust the position
                .attr("text-anchor", "middle")
                .text(d => d.value)
                .style("fill", "black")
                .style("font-size", "12px");

            // Add title
            svg.append("text")
                .attr("x", width / 2)
                .attr("y", 10)
                .attr("text-anchor", "middle")
                .style("font-size", "16px")
                .text("Line Chart Title");
        }
        else if (chartType === 'pie') {
            const radius = Math.min(chartWidth, chartHeight) / 2;

            const pie = d3.pie()
                .value(d => d.value);

            const arc = d3.arc()
                .innerRadius(0)
                .outerRadius(radius);

            const arcs = g.selectAll(".arc")
                .data(pie(chartData))
                .enter().append("g")
                .attr("class", "arc")
                .attr("transform", `translate(${chartWidth / 2}, ${chartHeight / 2})`);

            const color = d3.scaleOrdinal(d3.schemeCategory10);

            arcs.append("path")
                .attr("d", arc)
                .attr("fill", d => color(d.data.label));

            const legend = g.selectAll(".legend")
                .data(chartData.map(d => d.label))
                .enter().append("g")
                .attr("class", "legend")
                .attr("transform", (d, i) => `translate(50,${i * 20})`);

            legend.append("rect")
                .attr("x", chartWidth - 18)
                .attr("width", 18)
                .attr("height", 18)
                .attr("fill", (d, i) => color(i));

            legend.append("text")
                .attr("x", chartWidth - 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .style("text-anchor", "end")
                .text(d => (d.length > 12) ? d.substring(0, 12) + '...' : d)
                .on("mouseover", function () {
                    d3.select(this).text(d => d); // Show full text on mouseover
                })
                .on("mouseout", function () {
                    d3.select(this).text(d => (d.length > 12) ? d.substring(0, 12) + '...' : d); // Show ellipsis on mouseout
                });
        }
        else if (chartType === 'treemap') {
            const treemap = d3.treemap()
                .size([chartWidth, chartHeight])
                .padding(2);

            const root = d3.hierarchy({ children: chartData })
                .sum(d => d.value);

            treemap(root);

            const color = d3.scaleOrdinal(d3.schemeCategory10);

            const cell = g.selectAll("g")
                .data(root.leaves())
                .enter().append("g")
                .attr("transform", d => `translate(${d.x0},${d.y0})`);

            cell.append("rect")
                .attr("width", d => d.x1 - d.x0)
                .attr("height", d => d.y1 - d.y0)
                .attr("fill", d => color(d.parent.data.label));

            cell.append("text")
                .selectAll("tspan")
                .data(d => d.data.label.split(/(?=[A-Z][a-z])|\s+/))
                .enter().append("tspan")
                .attr("x", 4)
                .attr("y", (d, i) => 13 + i * 10)
                .text(d => d);
        }


        else {
            // Handle other chart types here
        }
    } else {
        alert("Failed to fetch data from the API for the chart.");
    }

    document.getElementById("chartOptions").style.display = "none";
}


async function insertTable() {
    const tableAPIURL = document.getElementById("tableAPIURL").value;
    const editor = document.getElementById("editor");

    const tableData = await fetchTableData(tableAPIURL);

    if (tableData) {
        const table = document.createElement("table");
        table.border = "1";
        const thead = table.createTHead();
        const tbody = table.createTBody();
        const headerRow = thead.insertRow();

        for (let key in tableData[0]) {
            const th = document.createElement("th");
            th.innerHTML = key;
            headerRow.appendChild(th);
        }

        tableData.forEach(rowData => {
            const row = tbody.insertRow();
            for (let key in rowData) {
                const cell = row.insertCell();
                cell.innerHTML = rowData[key];
            }
        });

        editor.appendChild(table);
        // Make the table draggable
        makeTableDraggable(table);
    } else {
        alert('Failed to fetch data from the API for the table.');
    }

    document.getElementById("tableOptions").style.display = "none";
}

async function fetchTableData(tableAPIURL) {
    try {
        const response = await fetch(tableAPIURL);
        if (response.ok) {
            const data = await response.json();
            return data;
        } else {
            throw new Error('Failed to fetch data for the table from the API');
        }
    } catch (error) {
        console.error(error);
        return null;
    }
}

function saveAsHTML() {
    const editorContent = document.getElementById("editor").innerHTML;
    const stylesheets = Array.from(document.styleSheets).map(styleSheet => {
        return `<link rel="stylesheet" href="${styleSheet.href}">`;
    }).join("\n");

    const htmlContent = `
        <!DOCTYPE html>
        <html>
        <head>
            <title>My Editor Content</title>
            ${stylesheets}
        </head>
        <body>
            <div id="editorContent">
                ${editorContent}
            </div>
        </body>
        </html>
    `;

    const blob = new Blob([htmlContent], { type: "text/html" });
    const a = document.createElement("a");
    a.href = URL.createObjectURL(blob);
    a.download = "editor_content.html";
    a.style.display = "none";
    document.body.appendChild(a);

    a.click();
    document.body.removeChild(a);
}


//
// ...


function makeChartDraggable(chartElement) {
    let offsetX = 0;
    let offsetY = 0;
    let isDragging = false;

    chartElement.style.position = 'absolute';
    chartElement.style.top = '0';
    chartElement.style.left = '0';

    chartElement.onmousedown = (event) => {
        isDragging = true;
        offsetX = event.clientX - chartElement.getBoundingClientRect().left;
        offsetY = event.clientY - chartElement.getBoundingClientRect().top;
    };

    document.onmousemove = (event) => {
        if (isDragging) {
            const x = event.clientX - offsetX;
            const y = event.clientY - offsetY;

            chartElement.style.left = x + 'px';
            chartElement.style.top = y + 'px';

            // Adjust the size of the editor div based on its content
            adjustEditorDimensions();
        }
    };

    document.onmouseup = () => {
        isDragging = false;
    };
}


function makeTableDraggable(tableElement) {
    let offsetX = 0;
    let offsetY = 0;
    let isDragging = false;

    tableElement.style.position = 'absolute';
    tableElement.style.top = '0';
    tableElement.style.left = '0';

    tableElement.onmousedown = (event) => {
        isDragging = true;
        offsetX = event.clientX - tableElement.getBoundingClientRect().left;
        offsetY = event.clientY - tableElement.getBoundingClientRect().top;
    };

    document.onmousemove = (event) => {
        if (isDragging) {
            const x = event.clientX - offsetX;
            const y = event.clientY - offsetY;

            tableElement.style.left = x + 'px';
            tableElement.style.top = y + 'px';

            // Adjust the size of the editor div based on its content
            adjustEditorDimensions();
        }
    };

    document.onmouseup = () => {
        isDragging = false;
    };
}

function adjustEditorDimensions() {
    const editor = document.getElementById("editor");
    const children = Array.from(editor.children);
    const totalHeight = children.reduce((height, element) => height + element.offsetHeight, 0);
    const totalWidth = children.reduce((width, element) => Math.max(width, element.offsetWidth), 0);

    // Add some extra padding or margins if needed
    const padding = 20;

    // Set the dimensions of the editor based on its content
    editor.style.height = totalHeight + padding + "px";
    editor.style.width = totalWidth + padding + "px";
}


