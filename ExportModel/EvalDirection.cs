﻿using Aveva.Pdms.Database;
using Aveva.Pdms.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportModel
{
	class EvalDirection
	{
		public static PointVector Eval(DbElement modelEle, string exper)
		{
			exper = exper.Trim().ToUpper();
			if (exper.Contains('P'))
			{
				bool isNeg = false;
				if (exper[0] == '-')
				{
					isNeg = true;
					exper = exper.Substring(1).Trim();
				}

				exper = exper.Substring(1);

				int num = int.Parse(exper);
				return MakeDirection(num, isNeg, modelEle);
			}
			else
			{
				return PointVector.Create(Position.Create(), Direction.Create(exper));
			}
		}

		private static PointVector MakeDirection(int num, bool isNeg, DbElement modelEle)
		{
			DbElement cate = modelEle.GetElement(DbAttributeInstance.SPRE).GetElement(DbAttributeInstance.CATR);
			DbElement ptre = cate.GetElement(DbAttributeInstance.PTRE);
			DbElement pt = ptre.FirstMember();
			while (pt != null && pt.IsValid)
			{
				if (num != pt.GetInteger(DbAttributeInstance.NUMB))
				{
					pt = pt.Next();
					continue;
				}

				DbElementType type = pt.GetElementType();
				if (type == DbElementTypeInstance.PTAXIS)
					return MakeAxialDirection(num, isNeg, pt, modelEle);
				else if (type == DbElementTypeInstance.PTCAR)
					return MakeCartesianDirection(num, isNeg, pt, modelEle);
				else if (type == DbElementTypeInstance.PTMIX)
					return MakeMixedDirection(num, isNeg, pt, modelEle);
				else if (type == DbElementTypeInstance.PTPOS)
					return MakePositionTypeDirection(num, isNeg, cate);
				pt = pt.Next();
			}
			return null;
		}

		private static PointVector MakeAxialDirection(int num, bool isNeg, DbElement pnt, DbElement modelEle)
		{
			string exper = pnt.GetAsString(DbAttributeInstance.PAXI);
			if (exper.Contains("DDANGLE"))
				exper = exper.Replace("DDANGLE", modelEle.GetAsString(DbAttributeInstance.ANGL));
			Direction dir = Direction.Create(exper);
			double offset = EvalDouble(pnt.GetAsString(DbAttributeInstance.PDIS), modelEle);
			if (isNeg)
				dir = dir.Opposite();
			Position pos = Position.Create();
			pos.MoveBy(dir, offset);
			return PointVector.Create(pos, dir);
		}

		private static double EvalDouble(string strExper, DbElement modelEle)
		{
			Experssion exper = new Experssion(strExper);
			return exper.Eval(modelEle);
		}

		private static PointVector MakeCartesianDirection(int num, bool isNeg, DbElement pnt, DbElement modelEle)
		{
			Direction dir = Direction.Create(pnt.GetAsString(DbAttributeInstance.PTCD));
			double x = EvalDouble(pnt.GetAsString(DbAttributeInstance.PX), modelEle);
			double y = EvalDouble(pnt.GetAsString(DbAttributeInstance.PY), modelEle);
			double z = EvalDouble(pnt.GetAsString(DbAttributeInstance.PZ), modelEle);
			return PointVector.Create(Position.Create(x, y, z), dir);
		}

		private static PointVector MakeMixedDirection(int num, bool isNeg, DbElement pnt, DbElement modelEle)
		{
			return MakeCartesianDirection(num, isNeg, pnt, modelEle);
		}

		private static PointVector MakePositionTypeDirection(int num, bool isNeg, DbElement cate)
		{
			return null;
		}
	}
}
