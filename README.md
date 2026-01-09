[![Build (Windows)](https://github.com/SAM-BIM/SAM_GEM/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/SAM-BIM/SAM_GEM/actions/workflows/build.yml)
[![Installer (latest)](https://img.shields.io/github/v/release/SAM-BIM/SAM_Deploy?label=installer)](https://github.com/SAM-BIM/SAM_Deploy/releases/latest)

# SAM_GEM

<a href="https://github.com/SAM-BIM/SAM">
  <img src="https://github.com/SAM-BIM/SAM/blob/master/Grasshopper/SAM.Core.Grasshopper/Resources/SAM_Small.png"
       align="left" hspace="10" vspace="6">
</a>

**SAM_GEM** is part of the **SAM (Sustainable Analytical Model) Toolkit** —  
an open-source collection of tools designed to help engineers create, manage,
and process analytical building models for energy and environmental analysis.

This repository provides utilities to **export SAM analytical geometry to `.gem` files**
for use in **IES Virtual Environment (IES VE)**.

A `.gem` file is a native VE file format that transfers **geometry and site location information only**.
It is typically used to move graphical model data between VE projects.

Welcome — and let’s keep the open-source journey going. 🤝

---

## Features

- Export of analytical geometry from SAM to **IES VE `.gem`**
- Export of **site location** metadata alongside geometry
- Support for transferring graphical model information between VE projects

---

## References

- IES VE help (DXF / GEM import): https://help.iesve.com/ve2018/dxf___gem_files.htm

---

## Resources
- 📘 **SAM Wiki:** https://github.com/SAM-BIM/SAM/wiki  
- 🧠 **SAM Core:** https://github.com/SAM-BIM/SAM  
- 🧰 **Installers:** https://github.com/SAM-BIM/SAM_Deploy  

---

## Installing

To install **SAM** using the Windows installer, download and run the  
[latest installer](https://github.com/SAM-BIM/SAM_Deploy/releases/latest).

Alternatively, you can build the toolkit from source using Visual Studio.  
See the main repository for details:  
👉 https://github.com/SAM-BIM/SAM

---

## Development notes

- Target framework: **.NET / C#**
- Export follows SAM-BIM analytical modelling conventions
- New or modified `.cs` files must include the SPDX header from `COPYRIGHT_HEADER.txt`

---

## Licence

This repository is free software licensed under the  
**GNU Lesser General Public License v3.0 or later (LGPL-3.0-or-later)**.

Each contributor retains copyright to their respective contributions.  
The project history (Git) records authorship and provenance of all changes.

See:
- `LICENSE`
- `NOTICE`
- `COPYRIGHT_HEADER.txt`
